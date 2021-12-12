using System;

namespace TKOM.ErrorHandler
{
    public class ErrorHandler : IErrorHandler
    {
        public ErrorHandler() { }

        public void HandleError(LexLocation location, string message)
        {
            Console.WriteLine($"{location.Start.Line}:{location.Start.Column} {message}");
        }

        public void HandleError(string message)
        {
            Console.WriteLine(message);
        }

        public void HandleWarning(LexLocation location, string message)
        {
            Console.WriteLine($"{location.Start.Line}:{location.Start.Column} {message}");
        }

        public void HandleWarning(string message)
        {
            Console.WriteLine(message);
        }
    }
}
