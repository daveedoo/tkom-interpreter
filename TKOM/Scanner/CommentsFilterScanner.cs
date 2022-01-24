using System.Collections;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public class CommentsFilterScanner : IScanner
    {
        private readonly IScanner scanner;
        public Token Current => scanner.Current;
        object IEnumerator.Current => Current;
        public string StringValue => scanner.StringValue;
        public int? IntValue => scanner.IntValue;

        public Position Position => scanner.Position;
        public IErrorHandler ErrorHandler => scanner.ErrorHandler;


        public CommentsFilterScanner(IScanner scanner)
        {
            this.scanner = scanner;
        }

        public bool MoveNext()
        {
            bool b = scanner.MoveNext();
            while (b && Current == Token.Comment)
            {
                b = scanner.MoveNext();
            }
            return b;
        }

        public void Reset() => scanner.Reset();
        public void Dispose() => scanner.Dispose();
    }
}
