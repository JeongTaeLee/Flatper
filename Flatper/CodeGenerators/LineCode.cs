using System;
using System.Collections.Generic;
using System.Text;

namespace Flatper
{
    public class LineCode : IChildCode
    {        
        private List<LineCode> _lineCodes = new List<LineCode>();

        public ICode parent { get; private set; } = null;
        public int indentCount => parent.indentCount + 1;
        public bool useIndent => parent.useIndent;

        public string name { get; private set; } = string.Empty;
        public string code { get; private set; } = string.Empty;

        public List<LineCode> lineCode => _lineCodes;        

        public LineCode(ICode prnt, string linName)
        {
            parent = prnt ?? throw new ArgumentNullException(nameof(prnt));
            name = linName;
        }

        public LineCode CreateLine(string name)
        {
            var linCode = new LineCode(this, name);
            _lineCodes.Add(linCode);

            return linCode;
        }

        public int RemoveLine(string name)
        {
            return _lineCodes.RemoveAll(lin => lin.name == name);
        }

        public LineCode SetCode(string cde)
        {
            code = cde;
            return this;
        }

        public void WriteCode(StringBuilder strBldr)
        {
            var idnt  = useIndent ? indentCount : 0;
            var nextIdnt = useIndent ? idnt + 1 : 0;

            var idntStr = CodeGeneratorUtil.CreateIndent(idnt);
            var nextIdntStr = CodeGeneratorUtil.CreateIndent(nextIdnt);
        
            strBldr.AppendLine(code.Indent(idntStr));
            
            var iter = _lineCodes.GetEnumerator();
            while (iter.MoveNext())
            {
                var curLine = iter.Current;
                curLine.WriteCode(strBldr);
            }
        }
    }
}