﻿
namespace TKOM.ErrorHandler
{
    public interface IErrorHandler
    {
        public void Error(LexLocation location, string message);
        public void Error(string message);
        public void Warning(LexLocation location, string message);
        public void Warning(string message);
    }
}
