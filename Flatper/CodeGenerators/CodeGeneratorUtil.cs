using System.Linq;

namespace Flatper
{
    public static class CodeGeneratorUtil
    {
        public static string CreateIndent(int idntCnt)
        {
            return string.Join("", Enumerable.Repeat("\t", idntCnt));
        }

        public static string Indent(this string str, string indentStr)
        {
            return $"{indentStr}{str}";
        }

        public static string CreateSerializerName(string serializer)
        {
            return $"{serializer}{CodeGeneratorConst.SERIALIZER_APPEND_NAME}";
        }

        public static string CreateDeserializerName(string deserializer)
        {
            return $"{deserializer}{CodeGeneratorConst.DESERIALIZER_APPEND_NAME}";
        }

        public static string ToAccessModifierStr(this AccessModifier accessModifier) => accessModifier switch
        {
            AccessModifier.Public => "public",
            AccessModifier.Protected => "protected",
            AccessModifier.Private => "private",
            AccessModifier.Internal => "internal",
            _ => "Error"
        };
    }
}