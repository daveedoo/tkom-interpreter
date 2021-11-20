namespace TKOM.Scanner
{
    public interface IScanner
    {
        public Token Current { get; }
        public string StringValue { get; }
        public int IntValue { get; }
        public bool MoveNext();
    }
}
