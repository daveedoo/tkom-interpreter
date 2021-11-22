namespace TKOM.Scanner
{
    public class CommentsFilterScanner : IScanner
    {
        private readonly IScanner scanner;
        public Token Current => scanner.Current;
        public string StringValue => scanner.StringValue;
        public int IntValue => scanner.IntValue;

        public uint LineNumber => scanner.LineNumber;
        public uint ColumnNumber => scanner.ColumnNumber;

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
    }
}
