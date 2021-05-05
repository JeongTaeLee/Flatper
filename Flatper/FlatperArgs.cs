using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Flatper
{
    public enum ArgsIdentifier
    {
        NONE = 0,
        INPUT,
        OUTPUT,
        COMPILER,
    }
    
    public class FlatperArgsFactory
    {
        public static IReadOnlyDictionary<string, Action<FlatperArgs, string>> parsers = new Dictionary<string, Action<FlatperArgs, string>>()
        {
            //{ArgsIdentifier.NONE, (args, str) => { throw new InvalidOperationException($"Invalid args \'{str}\'"); }},
            {FlatperConst.ARGS_INPUT_IDENTIFIER, (args, str) => { args.SetInput(str); }},
            {FlatperConst.ARGS_OUTPUT_IDENTIFIER, (args, str) => { args.SetOutput(str); }},
            {FlatperConst.ARGS_COMPILER_IDENTIFIER, (args, str) => { args.SetCompiler(str); }},
        };

        public static FlatperArgs Create(string[] args)
        {
            var flatperArgs = new FlatperArgs();

            var iter = args.GetEnumerator();
            while (iter.MoveNext())
            {
                var argsIdentifierStr = iter.Current.ToString();
                if (!parsers.TryGetValue(argsIdentifierStr, out var func))
                {
                    throw new InvalidOperationException($"Invalid args \'{argsIdentifierStr}\'");
                }

                if (iter.MoveNext())
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

        public void SetInput(string ipt) => input = ipt;
        public void SetOutput(string otpt) => output = otpt;
        public void SetCompiler(string cmpilr) => compiler = cmpilr;
    }
}