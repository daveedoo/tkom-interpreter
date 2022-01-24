using System.Collections.Generic;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public interface IScanner : IEnumerator<Token>
    {
        /// <summary>
        /// <c>string</c> representation of the current Token - if the Token has one.
        /// Null otherwise.
        /// </summary>
        public string StringValue { get; }
        /// <summary>
        /// <c>string</c> representation of the current Token - if the Token has one.
        /// Null otherwise.
        /// </summary>
        public int? IntValue { get; }

        /// <summary>
        /// Current position in the input stream.
        /// </summary>
        public Position Position { get; }
        /// <summary>
        /// Object used for handling errors occurred in the process of scanning the input.
        /// </summary>
        public IErrorHandler ErrorHandler { get; }
    }
}
