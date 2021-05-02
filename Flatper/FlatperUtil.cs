using System;
using System.Linq;
using System.Text;

namespace Flatper
{
    public abstract class CodeWriter : IDisposable
    {        
        public StringBuilder builder { get; private set; }
        public int indentCount { get; private set; }

        public string _indentStr {get; private set; }
        
        public CodeWriter(StringBuilder bldr, int idntCnt = 1)
        {
            builder = bldr;
            indentCount = idntCnt;
            _indentStr = string.Join("", Enumerable.Repeat("\t", indentCount));
        }

        public abstract void Dispose();

        public void WriteCode(string code)
        {
            builder.AppendLine(string.Format("{0}{1}", _indentStr, code));
        }

        public void WriteChildCode(string code)
        {
            WriteCode($"\t{code}");
        }
    }

    public class NamespaceWriter : CodeWriter
    {
        public NamespaceWriter(string ns, StringBuilder bldr, int idntCnt= 1)
            : base(bldr, idntCnt)
        {

            WriteCode(string.Format("namespace {0}", ns));
            WriteCode("{");
        }

        public void WriteUsing(string @using)
        {
            WriteChildCode(string.Format("using {0};", @using));
        }

        public override void Dispose()
        {
            WriteCode("}");
        }
    }

    public class ClassWriter : CodeWriter
    {
        public ClassWriter(string cls, StringBuilder bldr, int indent = 2)
            : base(bldr, indent)
        {
            WriteCode(string.Format("public static class {0}", cls));
            WriteCode("{");
        }

        public override void Dispose()
        {
            WriteCode("}");
        }
    }

    public class FuncWriter : CodeWriter
    {
        public FuncWriter(string func, StringBuilder bldr, int indent = 3)
            : base(bldr, indent)
        {
            WriteCode(func);
            WriteCode("{");
        }

        public override void Dispose()
        {
            WriteCode("}");
        }
    }


    public class FlatperUtil
    {
        
    }
}