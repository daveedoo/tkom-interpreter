using System.IO;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    /// <summary>
    /// Decorator around <see cref="TextReader"/> tracking the position
    /// (line and collumn) based on the read characters.
    /// </summary>
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

        public void SkipWhitespaces()
        {
            while (char.IsWhiteSpace(NextChar))
                Move();
        }
        public void SkipLettersAndDigits()
        {
            while (char.IsLetterOrDigit(NextChar))
                Move();
        }
        public void SkipDigits()
        {
            while (char.IsDigit(NextChar))
                Move();
        }
        public void SkipCurrentLine()
        {
            while (NextChar != '\n' && Move())
                ;
            Move();
        }
        public void SkipToQuoteOrNewline()
        {
            while (NextChar != '\n' && NextChar != '\"' && Move())
                ;
            Move();
        }
    }
}
