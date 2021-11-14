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
        Identifier,         // [a-zA-Z][a-zA-Z0-9]*
        IntConst,           // [-+]?(0|([1-9][0-9]*))
        Void                // "void"
    }

    public class Scanner : IEnumerator<Token>
    {

        object IEnumerator.Current => Current;
        public Token Current { get; private set; } = Token.Error;
        public string strValue;
        public int intValue;
        
        private readonly TextReader reader;
        private StringBuilder alreadyRead = new();

        public Scanner(TextReader reader)
        {
            this.reader = reader;
        }


        public bool MoveNext()
        {
            if (!SkipWhitespaces())
                return false;
            char ch = alreadyRead[0];

            if (char.IsLetter(ch))
            {
                switch (ch)     // keywords
                {
                    case 'v':
                        if (TryReadKeyword_OrIdentifier("void", 1)) return true;
                        break;
                    default:
                        break;
                }
                TryReadIdentifier();
            }
            else if (char.IsDigit(ch))
            {
                ReadIntConst();
            }
            return true;
        }

        private bool SkipWhitespaces()
        {
            char ch;
            do
            {
                int c = reader.Read();
                if (c < 0)
                    return false;
                ch = (char)c;
            } while (char.IsWhiteSpace(ch));
            alreadyRead.Append(ch);
            
            return true;
        }

        private bool TryReadKeyword_OrIdentifier(string keyword, int alreadyReadChars)
        {
            for (int i = alreadyReadChars; i < keyword.Length; i++)
            {
                int c = reader.Read();
                if (c < 0)
                    return false;

                char ch = (char)c;
                if (char.IsWhiteSpace(ch))  // if unable to find the keyword, return Identifier
                {
                    Current = Token.Identifier;
                    strValue = alreadyRead.ToString();
                    alreadyRead.Clear();
                    return true;
                }
                alreadyRead.Append(ch);
                if (c != keyword[i])
                    return false;
            }
            Current = Token.Void;
            alreadyRead.Clear();
            return true;
        }

        private bool TryReadIdentifier()
        {
            char ch = (char)reader.Read();
            while (char.IsLetterOrDigit(ch))   // TODO: co dla c = -1?
            {
                alreadyRead.Append(ch);
                ch = (char)reader.Read();
            }
            Current = Token.Identifier;
            strValue = alreadyRead.ToString();
            alreadyRead.Clear();
            if (!char.IsWhiteSpace(ch))
                alreadyRead.Append(ch);
            return true;
        }

        private void ReadIntConst()      // TODO: give some limit
            // TODO: leading zeros error
            // TODO: leading minus
        {
            char ch = (char)reader.Read();
            while (char.IsDigit(ch))
            {
                alreadyRead.Append(ch);
                ch = (char)reader.Read();
            }
            Current = Token.IntConst;
            intValue = Convert.ToInt32(alreadyRead.ToString());
            alreadyRead.Clear();
            if (!char.IsWhiteSpace(ch))
                alreadyRead.Append(ch);
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
