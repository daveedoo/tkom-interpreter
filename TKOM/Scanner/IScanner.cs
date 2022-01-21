using System.Collections.Generic;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public interface IScanner : IEnumerator<Token>
    {
        public string StringValue { get; }
        public int? IntValue { get; }

        public Position Position { get; }
        public IErrorHandler ErrorHandler { get; }
    }
}
