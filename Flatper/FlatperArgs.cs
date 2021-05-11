using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Flatper
{
    public class FlatperArgsFactory
    {
        public static IReadOnlyDictionary<string, Action<FlatperArgs, string>> parsers = new Dictionary<string, Action<FlatperArgs, string>>()
        {
            //{ArgsIdentifier.NONE, (args, str) => { throw new InvalidOperationException($"Invalid args \'{str}\'"); }},
            {FlatperConst.ARGS_INPUT_IDENTIFIER, (args, str) => { args.SetInput(str); }},
            {FlatperConst.ARGS_OUTPUT_IDENTIFIER, (args, str) => { args.SetOutput(str); }},
            {FlatperConst.ARGS_COMPILER_IDENTIFIER, (args, str) => { args.SetCompiler(str); }},

            {FlatperConst.ARGS_OPTION_GEN_OBJECT_API, (args, str) => { args.SetGenObjectApi(); }},
            {FlatperConst.ARGS_OPTION_WITHOUT_SERIALIZER, (args, str) => { args.SetWithoutSerializer(); }},
            {FlatperConst.ARGS_OPTION_WITHOUT_DESERIALIZER, (args, str) => { args.SetWithoutDeserializer(); }},
        };

        public static HashSet<string> optionArgs = new HashSet<string>()
        {
            FlatperConst.ARGS_OPTION_GEN_OBJECT_API,
            FlatperConst.ARGS_OPTION_WITHOUT_SERIALIZER,
            FlatperConst.ARGS_OPTION_WITHOUT_DESERIALIZER,
        };

        public static FlatperArgs Create(string[] args)
        {
            var flatperArgs = new FlatperArgs();

            var iter = args.GetEnumerator();
            while (iter.MoveNext())
            {
                var argsIdentifierStr = iter.Current.ToString().ToLower();
                if (!parsers.TryGetValue(argsIdentifierStr, out var func))
                {
                    throw new InvalidOperationException($"Invalid args \'{argsIdentifierStr}\'");
                }

                if (optionArgs.Contains(argsIdentifierStr))
                {
                    func?.Invoke(flatperArgs, string.Empty);
                }
                else if (iter.MoveNext())
                {
                    var argStr = iter.Current.ToString();
                    func?.Invoke(flatperArgs, argStr);
                }
            }

            return flatperArgs;
        }
    }

    public class FlatperArgs
    {
        public string input { get; private set; } = string.Empty;
        public string output { get; private set; } = string.Empty;
        public string compiler { get; private set; } = string.Empty;

        public bool genObjectApi { get; private set; } = false;
        public bool withoutSerializer { get; private set; } = false;
        public bool withoutDeserializer { get; private set; } = false;

        public void SetInput(string ipt) => input = ipt;
        public void SetOutput(string otpt) => output = otpt;
        public void SetCompiler(string cmpilr) => compiler = cmpilr;

        public void SetGenObjectApi() => genObjectApi = true;
        public void SetWithoutSerializer() => withoutSerializer = true;
        public void SetWithoutDeserializer() => withoutDeserializer = true;
        
    }
}