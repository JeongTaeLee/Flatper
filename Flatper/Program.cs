using System;

namespace Flatper
{
    class Program
    {
        static void Main(string[] args)
        {
            Flatper.RunAsync(args).Wait();
        }
    }
}
