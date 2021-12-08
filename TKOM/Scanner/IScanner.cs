using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public interface IScanner
    {
        public Token Current { get; }
        public string StringValue { get; }
        public int? IntValue { get; }

        public Position Position { get; }
        public IErrorHandler ErrorHandler { get; }

        /// <summary>
        /// Parses the next token from the TextReader. Return <c>true</c> on successful move (not necessarily valid token!).<br></br>
        /// Returns <c>false</c> only when there is nothing more to read,
        /// so it doesn't move further after recognition of <c>EOF</c> (returns <c>false</c> on attempt).
        /// If <c>Error</c> was recognized, proper error message with location is send to the <see cref="ErrorHandler"/>.
        /// </summary>
        public bool MoveNext();
    }
}
