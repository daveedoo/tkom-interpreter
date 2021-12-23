using System;

namespace TKOM.ErrorHandler
{
    public class ErrorHandler : IErrorHandler
    {
        public ErrorHandler() { }

        public void Error(LexLocation location, string message)
        {
            Console.WriteLine($"{location.Start.Line}:{location.Start.Column} {message}");
        }

        public void Error(string message)
        {
            Console.WriteLine(message);
        }

        public void Warning(LexLocation location, string message)
        {
            Console.WriteLine($"{location.Start.Line}:{location.Start.Column} {message}");
        }

        public void Warning(string message)
        {
            Console.WriteLine(message);
        }
    }
}
