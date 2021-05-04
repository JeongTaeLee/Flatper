using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flatper
{
    public record ParameterInfo(string parameterName, string parameterType);

    public class MethodCode : IChildCode
    {
        private List<ParameterInfo> _parameters = new List<ParameterInfo>();
        private List<LineCode> _lineCodes = new List<LineCode>();

        public ICode parent { get; private set; } = null;
        public int indentCount => parent.indentCount + 1;
        public bool useIndent => parent.useIndent;

        public AccessModifier accessModifier { get; private set; } = AccessModifier.Public;
        public bool isStatic { get; private set; } = false;
        public bool isExtensionsMethod { get; private set; } = false;
        public string returnType { get; private set; } = null;
        public string name { get; private set; } = string.Empty;

        public IReadOnlyList<ParameterInfo> parameters => _parameters;
        public List<LineCode> lineCode => _lineCodes;        
        public MethodCode(ClassCode prnt, string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            parent = prnt ?? throw new ArgumentNullException(nameof(prnt));
            name = methodName;
        }
        
        public LineCode CreateLine(string name = null)
        {
            var linCode = new LineCode(this, name);
            _lineCodes.Add(linCode);

            return linCode;
        }

        public int RemoveLine(string name)
        {
            return _lineCodes.RemoveAll(lin => lin.name == name);
        }

        public MethodCode AddParameter(string parameterName, string returnType)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (_parameters.Any(prmtr => prmtr.parameterName ==  parameterName))
            {
                throw new InvalidOperationException($"Method already added : MethodName{parameterName}");
            }

            _parameters.Add(new ParameterInfo(parameterName, returnType));

            return this;
        }

        public MethodCode RemoveParameter(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            _parameters.RemoveAll(prmtr => prmtr.parameterName == parameterName);

            return this;
        }

        public MethodCode SetAccessModifier(AccessModifier acesMdfr)
        {
            accessModifier = acesMdfr;
            return this;
        }

        public MethodCode SetIsStatic(bool isSttc)
        {
            isStatic = isSttc;
            return this;
        }

        public MethodCode SetExtensionMethod(bool isExtns)
        {
            isExtensionsMethod = isExtns;
            return this;
        }

        public MethodCode SetReturnType(string rtrnType)
        {
            if (string.IsNullOrEmpty(rtrnType))
            {
                throw new ArgumentNullException(nameof(rtrnType));
            }

            returnType = rtrnType;
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

            strBldr.Append(returnType);
            strBldr.Append(" ");

            strBldr.Append(name);

            // write parameter
            strBldr.Append("(");
            {
                if (isExtensionsMethod)
                {
                    strBldr.Append("this");
                    strBldr.Append(" ");        
                }

                for (int prmtrIdx = 0; prmtrIdx < _parameters.Count; ++prmtrIdx)
                {
                    var prmtr = _parameters[prmtrIdx];
                    
                    strBldr.Append(prmtr.parameterType);
                    strBldr.Append(" ");
                    strBldr.Append(prmtr.parameterName);

                    if ((_parameters.Count - 1) > prmtrIdx)
                    {
                        strBldr.Append(",");
                        strBldr.Append(" ");
                    }
                }
            }
            strBldr.Append(")");
            strBldr.AppendLine();

            strBldr.AppendLine("{".Indent(idntStr));
            {
                var iter = _lineCodes.GetEnumerator();
                while (iter.MoveNext())
                {
                    var curLine = iter.Current;
                    curLine.WriteCode(strBldr);
                }
            }
            strBldr.AppendLine("}".Indent(idntStr));
            strBldr.AppendLine();
        }
    }
}