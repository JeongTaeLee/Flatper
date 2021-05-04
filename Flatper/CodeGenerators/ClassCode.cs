using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flatper
{
    public class ClassCode : IChildCode
    {
        private List<MethodCode> _methodCodes = new List<MethodCode>();

        public ICode parent { get; private set; } = null;
        public int indentCount => parent.indentCount + 1;
        public bool useIndent => parent.useIndent;

        public AccessModifier accessModifier { get; private set; } = AccessModifier.Public;
        public bool isStatic { get; private set; } = false;
        public string name { get; private set; } = string.Empty;

        public IReadOnlyList<MethodCode> methodCodes => _methodCodes;

        public ClassCode(CodeRoot prnt, string clsName)
        {
            if (string.IsNullOrEmpty(clsName))
            {
                throw new ArgumentNullException(nameof(clsName));
            }

            parent = prnt ?? throw new ArgumentNullException(nameof(prnt));
            name = clsName;
        }

        public MethodCode CreateMethod(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_methodCodes.Any(cls => cls.name == name))
            {
                throw new InvalidOperationException($"Method already added: ClassName({name})");
            }

            var mthdCode = new MethodCode(this, name);
            _methodCodes.Add(mthdCode);

            return mthdCode;
        }

        public MethodCode RemoveMethod(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var removeMthd = _methodCodes.Find(mthd => mthd.name == name);
            if (removeMthd != null)
            {
                _methodCodes.Remove(removeMthd);
            }

            return removeMthd;
        }

        public ClassCode SetIsStatic(bool isSttc)
        {
            isStatic = isSttc;
            return this;
        }

        public ClassCode SetAccessModifier(AccessModifier acesMdfr)
        {
            accessModifier = acesMdfr;
            return this;
        }

        public void WriteCode(StringBuilder strBldr)
        {
            var idnt  = useIndent ? indentCount : 0;
            var nextIdnt = useIndent ? idnt + 1 : 0;

            var idntStr = CodeGeneratorUtil.CreateIndent(idnt);
            var nextIdntStr = CodeGeneratorUtil.CreateIndent(nextIdnt);

            strBldr.Append(accessModifier.ToAccessModifierStr().Indent(idntStr));
            strBldr.Append(" ");

            if (isStatic)
            {
                strBldr.Append(CodeGeneratorConst.STATIC_STR);
                strBldr.Append(" ");
            }

            strBldr.Append(CodeGeneratorConst.CLASS_STR);
            strBldr.Append(" ");

            strBldr.Append(name);
            strBldr.AppendLine();

            strBldr.AppendLine("{".Indent(idntStr));
            {
                var iter = _methodCodes.GetEnumerator();
                while (iter.MoveNext())
                {
                    var curMethod = iter.Current;
                    curMethod.WriteCode(strBldr);
                }
            }
            strBldr.AppendLine("}".Indent(idntStr));
        }
    }
}