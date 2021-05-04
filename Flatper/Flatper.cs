using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.CodeDom;
using System.CodeDom.Compiler;

namespace Flatper
{
    public class Flatper
    {
        public class FlatData
        {
            public string namespaceName;
            public List<string> tableNames = new List<string>();
        }

        public static async Task RunAsync(string[] args)
        {
            var option = FlatperArgs.Of(args);

            var flatData = await ParseFlatData(option.inputPath);
            
            await Task.WhenAll(GenerateSerializer(option, flatData), GenerateDeserializer(option, flatData));
        }

        private static async Task<FlatData> ParseFlatData(string flatFilePath)
        {
            var flatFileText = await File.ReadAllTextAsync(flatFilePath);

            // 주석 제거.
            

            // 개행 문자 제거.            
            flatFileText = flatFileText.Replace('\r', ' ');
            flatFileText = flatFileText.Replace('\n', ' ');
            flatFileText = Regex.Replace(flatFileText, @"\{(.+?)\}", " ");
            flatFileText = Regex.Replace(flatFileText,  @"[^a-zA-Z0-9가-힣\.]", " ");

            var stubs = flatFileText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var flatData = new FlatData();

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
                }
            }

            return flatData;
        }

        private static async Task GenerateSerializer(FlatperArgs flatArgs, FlatData flatData)
        {
            var codeRoot = new CodeRoot();
            codeRoot.AddUsing(flatData.namespaceName)
                .AddUsing("FlatBuffers")
                .SetNamespace(flatData.namespaceName);

            var serializerName = Path.GetFileNameWithoutExtension(flatArgs.inputPath);
            var className = CodeGeneratorUtil.CreateSerializerName(serializerName);

            var classCode = codeRoot.CreateClass(className)
                .SetIsStatic(true);
            
            foreach (var tableName in flatData.tableNames)
            {
                var methodCode = classCode.CreateMethod($"Serialize{tableName}")
                    .SetIsStatic(true)
                    .SetReturnType("byte[]")
                    .AddParameter($"tb", $"{tableName}T");

                methodCode.CreateLine()
                    .SetCode($"var fbb = new FlatBufferBuild(1);");
            
                methodCode.CreateLine()
                    .SetCode($"fbb.Finish({tableName}.Pack(fbb, tb).Value);");

                methodCode.CreateLine()
                    .SetCode("return fbb.SizedByteArray();");
            }

            var savePath = Path.Combine(flatArgs.outputFolderPath, className + ".cs");
            await File.WriteAllTextAsync(savePath, codeRoot.BuildCode().ToString());
        }

        private static async Task GenerateDeserializer(FlatperArgs flatArgs, FlatData flatData)
        {
            var codeRoot = new CodeRoot();
            codeRoot.AddUsing(flatData.namespaceName)
                .AddUsing("FlatBuffers")
                .SetNamespace(flatData.namespaceName);

            var deserializerName = Path.GetFileNameWithoutExtension(flatArgs.inputPath);
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

            var savePath = Path.Combine(flatArgs.outputFolderPath, className + ".cs");
            await File.WriteAllTextAsync(savePath, codeRoot.BuildCode().ToString());
        }
    }
}