using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSGenerator;
using System.Diagnostics;

namespace Flatper
{
    public class Flatper
    {
        public class ParsedFlatSchema
        {
            public string namespaceName;
            public List<string> tableNames = new List<string>();
        }

        public static async Task RunAsync(string[] args)
        {
            var flatperArgs = FlatperArgsFactory.Create(args);

            await CompileFlat(flatperArgs);

            var flatData = await ParseFlatSchema(flatperArgs);

            Task srlzrTask = null;
            Task DsrlzrTask = null;
            Task genUnionHelperTask = null;

            if (!flatperArgs.withoutSerializer)
            {
                srlzrTask = GenerateSerializer(flatperArgs, flatData);
            }

            if (!flatperArgs.withoutDeserializer)
            {
                DsrlzrTask = GenerateDeserializer(flatperArgs, flatData);
            }

            await Task.WhenAll(new List<Task> { srlzrTask, DsrlzrTask, genUnionHelperTask });

            Console.WriteLine("Done.");
        }

        private static async Task CompileFlat(FlatperArgs args)
        {
            var outputFolder = Path.GetFullPath(args.output);

            // 파일 체크.
            var di = new DirectoryInfo(args.output);

            var files = di.GetFiles("*", SearchOption.AllDirectories);
            var dirs = di.GetDirectories();
            if (0 < (files.Length + dirs.Length))
            {
                Console.WriteLine($"!!주의!! 출력 폴더인 \'{outputFolder}\'에 파일이 존재합니다. 모두 지우고 다시 생성합니다...(아무키나 입력해주세요)");
                Console.ReadKey();
            
                Console.WriteLine("========== DELETE OLD FILES & FOLDER ==========");
                // 모든 파일 제거.
                if (0 < files.Length)
                {
                    foreach (var file in files)
                    {
                        file.Delete();
                        Console.WriteLine($"Delete File - {file.Name}");
                    }
                }

                // 모든 폴더 제거.
                if (0 < dirs.Length)
                {
                    foreach (var dir in dirs)
                    {
                        dir.Delete(true);
                        Console.WriteLine($"Delete Folder - {dir.Name}");
                    }
                }
                Console.WriteLine("===============================================");
                Console.WriteLine("");
            }

            var compilerArgs = $"--gen-object-api --csharp -o {args.output} {args.input}";
            Console.WriteLine("============ START FLATC COMPILER =============");
            Console.WriteLine($"EX : {args.compiler} {compilerArgs}");
            Console.WriteLine("===============================================");
            Console.WriteLine("");

            var ps = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = args.compiler,
                    UseShellExecute = true,
                    Arguments = compilerArgs,
                }
            };
            
            ps.Start();
            await ps.WaitForExitAsync();
            
            Console.WriteLine("===============================================");
            Console.WriteLine("");
        }

        private static async Task<ParsedFlatSchema> ParseFlatSchema(FlatperArgs args)
        {
            Console.WriteLine("========== START PARSING FLAT SCHEMA ==========");

            var flatFileText = await File.ReadAllTextAsync(args.input);

            // 개행 문자 제거.            
            flatFileText = flatFileText.Replace('\r', ' ');
            flatFileText = flatFileText.Replace('\n', ' ');
            flatFileText = Regex.Replace(flatFileText, @"\{(.+?)\}", " ");
            flatFileText = Regex.Replace(flatFileText,  @"[^a-zA-Z0-9가-힣\.]", " ");

            var stubs = flatFileText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var flatData = new ParsedFlatSchema();

            var iter = stubs.GetEnumerator();
            while (iter.MoveNext())
            {
                var stub = iter.Current as string;

                if (stub == "namespace")
                {
                    if (!iter.MoveNext())
                    {
                        continue;
                    }

                    stub = iter.Current as string;
                    if (string.IsNullOrEmpty(stub))
                    {
                        continue;
                    }

                    flatData.namespaceName = stub;
                    Console.WriteLine($"Namepsace parsing done - {flatData.namespaceName}");
                }

                if (stub == "table")
                {
                    if (!iter.MoveNext())
                    {
                        continue;
                    }

                    stub = iter.Current as string;
                    if (string.IsNullOrEmpty(stub))
                    {
                        continue;
                    }

                    
                    flatData.tableNames.Add(stub);
                       Console.WriteLine($"Table parsing done - {stub}");
                }
            }

            Console.WriteLine("===============================================");

            return flatData;
        }

        private static async Task GenerateSerializer(FlatperArgs flatArgs, ParsedFlatSchema flatData)
        {
            Console.WriteLine("========== Start generate serializer ==========");

            var codeRoot = new CodeRoot();
            codeRoot.AddUsing(flatData.namespaceName)
                .AddUsing("FlatBuffers")
                .SetNamespace(flatData.namespaceName);

            var serializerName = Path.GetFileNameWithoutExtension(flatArgs.input);
            var className = CodeGeneratorUtil.CreateSerializerName(serializerName);

            var classCode = codeRoot.CreateClass(className)
                .SetIsStatic(true);
            
            foreach (var tableName in flatData.tableNames)
            {
                var methodCode = classCode.CreateMethod($"Serialize")
                    .SetIsStatic(true)
                    .SetReturnType("byte[]")
                    .SetExtensionMethod(true)
                    .AddParameter($"tb", $"{tableName}T");

                methodCode.CreateLine()
                    .SetCode($"var fbb = new FlatBufferBuilder(1);");
            
                methodCode.CreateLine()
                    .SetCode($"fbb.Finish({tableName}.Pack(fbb, tb).Value);");

                methodCode.CreateLine()
                    .SetCode("return fbb.SizedByteArray();");
            }

            var savePath = Path.Combine(flatArgs.output, className + ".cs");
            await File.WriteAllTextAsync(savePath, codeRoot.BuildCode().ToString());

            Console.WriteLine($"Generate complete : {savePath}");

            Console.WriteLine("===============================================");
            Console.WriteLine("");
        }

        private static async Task GenerateDeserializer(FlatperArgs flatArgs, ParsedFlatSchema flatData)
        {
            Console.WriteLine("========== Start generate Deserializer ==========");

            var codeRoot = new CodeRoot();
            codeRoot.AddUsing(flatData.namespaceName)
                .AddUsing("FlatBuffers")
                .SetNamespace(flatData.namespaceName);

            var deserializerName = Path.GetFileNameWithoutExtension(flatArgs.input);
            var className = CodeGeneratorUtil.CreateDeserializerName(deserializerName);

            var classCode = codeRoot.CreateClass(className)
                .SetIsStatic(true);
            
            foreach (var tableName in flatData.tableNames)
            {
                var methodCode = classCode.CreateMethod($"Deserialize{tableName}")
                    .SetIsStatic(true)
                    .SetReturnType($"{tableName}T")
                    .AddParameter($"buffer", $"byte[]");

                methodCode.CreateLine()
                    .SetCode($"var bb = new ByteBuffer(buffer);");
            
                methodCode.CreateLine()
                    .SetCode($"var cc = {tableName}.GetRootAs{tableName}(bb);");

                methodCode.CreateLine()
                    .SetCode("return cc.UnPack();");
            }

            var savePath = Path.Combine(flatArgs.output, className + ".cs");
            await File.WriteAllTextAsync(savePath, codeRoot.BuildCode().ToString());

            Console.WriteLine($"Generate complete : {savePath}");

            Console.WriteLine("===============================================");
            Console.WriteLine("");
        }
    }
}