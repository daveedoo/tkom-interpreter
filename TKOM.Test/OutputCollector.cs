using System.IO;
using System.Text;

namespace TKOMTest
{
    public class OutputCollector : TextWriter
    {
        public override Encoding Encoding => Encoding.Default;
        private readonly StringBuilder stringBuilder = new();

        public OutputCollector() : base()
        { }

        public override void Write(char value)
        {
            stringBuilder.Append(value);
        }
        public string GetOutput()
        {
            return stringBuilder.ToString();
        }
        public void Clear()
        {
            stringBuilder.Clear();
        }
    }
}
