using System.IO;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    internal class PositionTrackingTextReader
    {
        private readonly TextReader reader;
        public Position Position { get; }
        public bool eof { get; private set; }
        public char NextChar { get; private set; }

        public PositionTrackingTextReader(TextReader reader)
        {
            this.reader = reader;
            Position = new Position(1, 0);
            eof = false;
            updateNextChar();
        }

        public bool Move()
        {
            if (eof)
                return false;

            int nextChar = reader.Read();
            if (nextChar == '\n')
                Position.IncrementLine();
            else
                Position.IncrementColumn();

            updateNextChar();
            return !eof;
        }

        private void updateNextChar()
        {
            if (eof)
                return;

            int nextChar = reader.Peek();
            if (reader.Peek() < 0)
                eof = true;
            NextChar = (char)nextChar;
        }
    }
}
