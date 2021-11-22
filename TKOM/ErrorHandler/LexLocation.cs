
namespace TKOM.ErrorHandler
{
    public struct LexLocation
    {
        public readonly uint StartLine;
        public readonly uint StartColumn;
        public readonly uint EndLine;
        public readonly uint EndColumn;

        public LexLocation(uint startLine, uint startColumn, uint endLine, uint endColumn)
        {
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }
    }
}
