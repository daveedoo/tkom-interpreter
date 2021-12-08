
namespace TKOM.ErrorHandler
{
    public struct LexLocation
    {
        public Position Start;
        public Position End;

        public LexLocation(uint startLine, uint startColumn, uint endLine, uint endColumn)
        {
            Start = new Position(startLine, startColumn);
            End = new Position(endLine, endColumn);
        }

        public LexLocation(Position start, Position end)
        {
            Start = start;
            End = end;
        }
    }
}
