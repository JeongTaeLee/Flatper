using System.Text;

namespace Flatper
{
    public interface IChildCode : ICode
    {
        ICode parent { get; }

        void WriteCode(StringBuilder strBldr);
    }
}