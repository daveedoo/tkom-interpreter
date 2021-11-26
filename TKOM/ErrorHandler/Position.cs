
namespace TKOM.ErrorHandler
{
    public struct Position
    {
        public uint Line;
        public uint Column;

        public Position(uint line, uint column)
        {
            Line = line;
            Column = column;
        }

        public void IncrementLine()
        {
            Line++;
            Column = 0;
        }
        public void IncrementColumn()
        {
            Column++;
        }
    }
}
