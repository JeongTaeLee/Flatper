using System;
using System.Collections;
using System.IO;

namespace Flatper
{
    public readonly struct FlatperArgs
    {
        public string inputPath { get; }
        public string outputFolderPath { get; }
        public bool genOneFile { get; }

        private FlatperArgs(string inputPath, string outputFolderPath, bool genOneFile)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                throw new ArgumentNullException(nameof(inputPath));
            }

            if (string.IsNullOrEmpty(outputFolderPath))
            {
                throw new ArgumentNullException(nameof(outputFolderPath));
            }

            this.inputPath = inputPath;
            this.outputFolderPath = outputFolderPath;
            this.genOneFile = genOneFile;
        }

        public static FlatperArgs Of(string[] args)
        {
            var compilerPath = string.Empty;
            var inputPath = string.Empty;
            var outputFolderPath = string.Empty;
            var genOneFile = false;

            var iter = args.GetEnumerator();
            while (iter.MoveNext())
            {
                string str = iter.Current as string;

                str = str.ToLower();

                switch (str)
                {
                    case "--i":
                    {
                        if (!TryParseOption(iter, out inputPath) || !File.Exists(inputPath))
                        {
                            throw new ArgumentException("Invalid flat file path");
                        }
                        break;
                    }

                    case "--o":
                    {
                        if (!TryParseOption(iter, out outputFolderPath) || !Directory.Exists(outputFolderPath))
                        {
                            throw new ArgumentException("Invalid output folder path");
                        }
                        break;
                    }

                    case "--gen-onefile":
                    {
                        genOneFile = true;
                        break;
                    }

                }
            }

            return new FlatperArgs( inputPath, outputFolderPath, genOneFile);
        } 

        private static bool TryParseOption(IEnumerator iter, out string str)
        {
            str = string.Empty;

            if (!iter.MoveNext())
            {
                return false;
            }

            str = iter.Current as string;
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Invalid CompilerPath");
            }

            return true;
        }
    }
}