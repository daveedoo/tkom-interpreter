using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public interface IScanner
    {
        public Token Current { get; }
        public string StringValue { get; }
        public int IntValue { get; }

        public Position Position { get; }
        public IErrorHandler ErrorHandler { get; }

        /// <summary>
        /// Parses the next token from the TextReader. Returns <c>true</c> when the token read was valid.
        /// Returns <c>false</c> in few case:
        /// <list type="bullet">
        ///     <item>EOF enocuntered.</item>
        ///     <item>Unknown token / character encountered.</item>
        ///     <item>Token was too long or invalid in other way.</item>
        /// </list>
        /// If false was returned not (or not only) because EOF was encountered,
        /// proper error message with location is send to the <see cref="ErrorHandler"/>.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext();
    }
}
