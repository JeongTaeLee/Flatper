using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Flatper
{
    public class CodeRoot : ICode
    {
        private List<string> _usings = new List<string>();
        private List<ClassCode> _classCodes = new List<ClassCode>();
        
        public int indentCount { get; private set; } = 0;
        public bool useIndent { get; private set; } = true;

        public string namespaceName {get; private set; } = string.Empty;
        
        public IReadOnlyList<string> usings => _usings;
        public IReadOnlyList<ClassCode> classCodes => _classCodes;
        public CodeRoot AddUsing(string @using)
        {
            if (string.IsNullOrEmpty(@using))
            {
                throw new ArgumentNullException(nameof(@using));
            }

            if (_usings.Any(us => us == @using))
            {
                throw new InvalidOperationException($"Using already added : Using({@using})");
            }

            _usings.Add(@using);
            return this;
        }

        public CodeRoot RemoveUsing(string @using)
        {
            if (string.IsNullOrEmpty(@using))
            {
                throw new ArgumentNullException(nameof(@using));
            }

            _usings.Remove(@using);
            return this;
        }

        public ClassCode CreateClass(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_classCodes.Any(cls => cls.name == name))
            {
                throw new InvalidOperationException($"Class already added: ClassName({name})");
            }

            var clsCode = new ClassCode(this, name);
            _classCodes.Add(clsCode);

            return clsCode;
        }

        public ClassCode RemoveClass(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var removeCls = _classCodes.Find(cls => cls.name == name);
            if (removeCls != null)
            {
                _classCodes.Remove(removeCls);
            }

            return removeCls;
        }

        public CodeRoot SetNamespace(string nsName)
        {
            namespaceName = nsName;
            return this;
        }

        public CodeRoot SetIndentCount(int idntCnt)
        {
            indentCount = idntCnt;
            return this;
        }

        public StringBuilder BuildCode()
        {
            var strBldr = new StringBuilder();
            var idnt  = useIndent ? indentCount : 0;
            var nextIdnt = useIndent ? idnt + 1 : 0;

            var idntStr = CodeGeneratorUtil.CreateIndent(idnt);
            var nextIdntStr = CodeGeneratorUtil.CreateIndent(nextIdnt);
            
            // Write Usings
            foreach (var @using in _usings)
            {
                strBldr.AppendLine($"using {@using};".Indent(idntStr));
            }

            strBldr.AppendLine();

            // write namespace
            bool hasNamespace = !string.IsNullOrEmpty(namespaceName); 

            if (hasNamespace)
            {
                strBldr.AppendLine($"namespace {namespaceName}".Indent(idntStr));
                strBldr.AppendLine("{".Indent(idntStr));
            }
            
            {
                var iter = _classCodes.GetEnumerator();
                while (iter.MoveNext())
                {
                    var classCode = iter.Current;

                    classCode.WriteCode(strBldr);
                }
            }
            
            if (hasNamespace)
            {
                strBldr.AppendLine("}".Indent(idntStr));
            }

            return strBldr;
        }
    }
}