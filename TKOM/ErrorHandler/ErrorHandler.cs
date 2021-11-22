using System;

namespace TKOM.ErrorHandler
{
    public class ErrorHandler : IErrorHandler
    {
        public ErrorHandler() { }

        public void HandleError(LexLocation location, string message)
        {
            Console.WriteLine($"{location.StartLine}:{location.StartColumn} {message}");
        }

        public void HandleWarning(LexLocation location, string message)
        {
            Console.WriteLine($"{location.StartLine}:{location.StartColumn} {message}");
        }
    }
}
