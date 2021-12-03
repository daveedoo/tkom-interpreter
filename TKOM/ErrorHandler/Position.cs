
namespace TKOM.ErrorHandler
{
    public record Position
    {
        public uint Line { get;private set; }
        public uint Column { get; private set; }

        public Position(uint line, uint column)
        {
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Increment line counter and reset column counter.
        /// </summary>
        public void IncrementLine()
        {
            Line++;
            Column = 0;
        }
        /// <summary>
        /// Increment column counter
        /// </summary>
        public void IncrementColumn()
        {
            Column++;
        }
    }
}
