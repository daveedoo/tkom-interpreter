
namespace TKOM.ErrorHandler
{
    public interface IErrorHandler
    {
        public void HandleError(LexLocation location, string message);
        public void HandleError(string message);
        public void HandleWarning(LexLocation location, string message);
        public void HandleWarning(string message);
    }
}
