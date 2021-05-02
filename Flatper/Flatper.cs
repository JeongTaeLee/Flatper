using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flatper
{
    public class Flatper
    {
        public class FlatData
        {
            public string ns;
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

                    flatData.ns = stub;
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
            var strBldr = new StringBuilder();
            var name = Path.GetFileNameWithoutExtension(flatArgs.outputFolderPath) + "Serializer";

            using (var nw = new NamespaceWriter(flatData.ns, strBldr))
            {
                nw.WriteUsing("Flatbuffers");
                nw.WriteUsing(flatData.ns);

                var className = Path.GetFileNameWithoutExtension(flatArgs.outputFolderPath) + "Serializer";

                using (var cw = new ClassWriter(className, strBldr))
                {
                    foreach (var tableName in flatData.tableNames)
                    {
                        var func = string.Format("public static byte[] Serializer{0}({1}T flat)", tableName, tableName);
                        using (var fw = new FuncWriter(func, strBldr))
                        {   
                            fw.WriteChildCode("var fbb = new FlatBufferBuilder(1);");
                            fw.WriteChildCode(string.Format("fbb.Finish({0}.Pack(fbb, flat).Value);", tableName));
                            fw.WriteChildCode("return fbb.SizedByteArray();");
                        }
                    }
                }
            }

            var filePath = Path.Combine(flatArgs.outputFolderPath, $"{name}.cs");

            await File.WriteAllTextAsync(filePath, strBldr.ToString());
        }

        private static async Task GenerateDeserializer(FlatperArgs flatArgs, FlatData flatData)
        {
            var strBldr = new StringBuilder();
            var name = Path.GetFileNameWithoutExtension(flatArgs.outputFolderPath) + "Deserializer";

            using (var nw = new NamespaceWriter(flatData.ns, strBldr))
            {
                nw.WriteUsing("Flatbuffers");
                nw.WriteUsing(flatData.ns);

                var className = Path.GetFileNameWithoutExtension(flatArgs.outputFolderPath) + "Deserializer";

                using (var cw = new ClassWriter(className, strBldr))
                {
                    foreach (var tableName in flatData.tableNames)
                    {
                        var func = string.Format("public static {0}T Deserializer{1}(byte[] buffer)", tableName, tableName);
                        using (var fw = new FuncWriter(func, strBldr))
                        {   
                            fw.WriteChildCode("var bb = new Byte(buffer);");
                            fw.WriteChildCode(string.Format("var cc ={0}.GetRootAs{1};", tableName, tableName));
                            fw.WriteChildCode("return cc.UnPack();");
                        }
                    }
                }
            }

            var filePath = Path.Combine(flatArgs.outputFolderPath, $"{name}.cs");

            await File.WriteAllTextAsync(filePath, strBldr.ToString());
        }

    }
}