using System.Text;

namespace Flatper
{
    public interface ICode
    {
        int indentCount { get; }// => parent.startIndentCount + 1;
        bool useIndent { get; }// parent.useIndent;
    }
}