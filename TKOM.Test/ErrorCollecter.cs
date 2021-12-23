using System.Collections.Generic;
using TKOM.ErrorHandler;

namespace TKOMTest
{
    public class ErrorCollecter : IErrorHandler
    {
        private List<(LexLocation, string)> errorsList = new List<(LexLocation, string)>();
        private List<string> noLocationErrorsList = new List<string>();
        public int errorCount => errorsList.Count + noLocationErrorsList.Count;

        private List<(LexLocation, string)> warningsList = new List<(LexLocation, string)>();
        private List<string> noLocationWarningsList = new List<string>();
        public int warningsCount => warningsList.Count + noLocationWarningsList.Count;

        public ErrorCollecter() { }

        public void Error(LexLocation location, string message)
        {
            errorsList.Add((location, message));
        }

        public void Warning(LexLocation location, string message)
        {
            warningsList.Add((location, message));
        }

        public (LexLocation location, string message)? GetLastError()
        {
            if (errorCount == 0)
                return null;
            return errorsList[errorCount - 1];
        }

        public (LexLocation location, string message)? GetLastWarning()
        {
            if (warningsCount == 0)
                return null;
            return warningsList[warningsCount - 1];
        }

        public void Error(string message)
        {
            noLocationErrorsList.Add(message);
        }

        public void Warning(string message)
        {
            noLocationWarningsList.Add(message);
        }

        public void Error(Position position, string message)
        {
            Error(new LexLocation(position, position), message);
        }

        public void Warning(Position position, string message)
        {
            Warning(new LexLocation(position, position), message);
        }
    }
}
