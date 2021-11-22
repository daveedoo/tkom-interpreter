using System.Collections.Generic;
using TKOM.ErrorHandler;

namespace TKOM.Scanner.Test
{
    internal class ErrorCollecter : IErrorHandler
    {
        private List<(LexLocation, string)> errorsList = new List<(LexLocation, string)>();
        public int errorCount => errorsList.Count;
        private List<(LexLocation, string)> warningsList = new List<(LexLocation, string)>();
        public int warningsCount => warningsList.Count;

        public ErrorCollecter() { }

        public void HandleError(LexLocation location, string message)
        {
            errorsList.Add((location, message));
        }

        public void HandleWarning(LexLocation location, string message)
        {
            warningsList.Add((location, message));
        }
    }
}
