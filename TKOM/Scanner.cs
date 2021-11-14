using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TKOM
{
    public enum Token
    {
        Error,
        Identifier
    }

    public class Scanner : IEnumerator<Token>
    {
        private readonly TextReader reader;

        object IEnumerator.Current => Current;
        public Token Current { get; private set; }
        public string strValue;
        public int intValue;

        private bool eof;

        public Scanner(TextReader reader)
        {
            this.reader = reader;
            eof = false;
        }

        public bool MoveNext()
        {
            int c;
            do
            {
                c = reader.Read();
                if (c < 0)
                {
                    eof = true;
                    return false;
                }
            } while (char.IsWhiteSpace((char)c));
            StringBuilder builder = new();

            if (char.IsLetter((char)c))
            {
                do
                {
                    builder.Append((char)c);
                    c = reader.Read();
                } while (char.IsLetterOrDigit((char)c));
                Current = Token.Identifier;
                strValue = builder.ToString();
            }
            else if (char.IsDigit((char)c))
            {
            }
            else
            {
            }
            return true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            throw new NotSupportedException();
        }
    }
}
