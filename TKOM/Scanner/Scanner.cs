using System.IO;
using System.Text;

namespace TKOM.Scanner
{
    public class Scanner : IScanner
    {
        private readonly TextReader reader;
        public Token Current { get; private set; }

        private bool eof = false;
        private int nextChar;
        public string StringValue { get; private set; }
        public int IntValue { get; private set; }

        public uint LineNumber { get; private set; }
        public uint ColumnNumber { get; private set; }

        public Scanner(TextReader reader)
        {
            this.reader = reader;
            Current = Token.Error;
            LineNumber = 1;
            ColumnNumber = 0;
            readNextChar();
        }

        public bool MoveNext()
        {
            if (eof)
                return false;

            skipWhitespaces();
            char ch = (char)nextChar;

            StringBuilder buffer = new();
            buffer.Append(ch);
            if (char.IsLetter(ch))
            {
                readWhileLetterOrDigit(buffer);
                StringValue = buffer.ToString();
                Current = StringValue switch
                {
                    "void" => Token.Void,
                    "int"           => Token.Int,
                    "return"        => Token.Return,
                    "if"            => Token.If,
                    "else"          => Token.Else,
                    "while"         => Token.While,
                    "read"          => Token.Read,
                    "print"         => Token.Print,
                    "try"           => Token.Try,
                    "catch"         => Token.Catch,
                    "finally"       => Token.Finally,
                    "throw"         => Token.Throw,
                    "when"          => Token.When,
                    "Exception"     => Token.Exception,
                    _ => Token.Identifier
                };
            }
            else if (char.IsDigit(ch))
            {
                readWhileDigit(buffer);
                IntValue = int.Parse(buffer.ToString());
                Current = Token.IntConst;
            }
            else if (nextChar < 0)
            {
                ColumnNumber++;
                eof = true;
                return false;
            }
            else
            {
                switch (ch)
                {
                    case '|': Current = tryReadOrToken(); break;
                    case '&': Current = tryReadAndToken(); break;
                    case '/': Current = tryReadCommentToken(); break;
                    case '"': Current = readStringToken(); break;
                    default:
                        Current = ch switch
                        {
                            '(' => Token.RoundBracketOpen,
                            ')' => Token.RoundBracketClose,
                            '{' => Token.CurlyBracketOpen,
                            '}' => Token.CurlyBracketClose,
                            '-' => Token.Minus,
                            '+' => Token.Plus,
                            '*' => Token.Star,
                            '<' => Token.LessThan,
                            '>' => Token.GreaterThan,
                            '=' => Token.Equals,
                            '!' => Token.Not,
                            ';' => Token.Semicolon,
                            ',' => Token.Comma,
                            '.' => Token.Dot,
                            _ => Token.Error
                        };
                        readNextChar();
                        break;
                }
            }
            return true;
        }

        private void skipWhitespaces()
        {
            while (char.IsWhiteSpace((char)nextChar))
            {
                if (nextChar == '\n')
                {
                    LineNumber++;
                    ColumnNumber = 0;
                }
                readNextChar();
            }
                //ColumnNumber++;
        }

        private void readWhileLetterOrDigit(StringBuilder buffer)
        {
            readNextChar();
            char ch = (char)nextChar;
            while (char.IsLetterOrDigit(ch))
            {
                buffer.Append(ch);
                readNextChar();
                ch = (char)nextChar;
            }
        }

        private void readWhileDigit(StringBuilder buffer)
        {
            readNextChar();
            char ch = (char)nextChar;
            while (char.IsDigit(ch))
            {
                buffer.Append(ch);
                readNextChar();
                ch = (char)nextChar;
            }
        }

        private Token tryReadOrToken()
        {
            readNextChar();
            switch (nextChar)
            {
                case '|':
                    readNextChar();
                    return Token.Or;
                default:
                    return Token.Error;
            }
        }

        private Token tryReadAndToken()
        {
            readNextChar();
            switch (nextChar)
            {
                case '&':
                    readNextChar();
                    return Token.And;
                default:
                    return Token.Error;
            }
        }

        private Token tryReadCommentToken()
        {
            readNextChar();
            if (nextChar == '/')
            {
                StringBuilder buffer = new();
                readNextChar();
                while (nextChar >= 0 && nextChar != '\n')
                {
                    buffer.Append((char)nextChar);
                    readNextChar();
                }
                StringValue = buffer.ToString();
                return Token.Comment;
            }
            return Token.Slash;
        }

        private Token readStringToken()
        {
            StringBuilder buffer = new();
            readNextChar();
            while (nextChar >= 0 && nextChar != '"')
            {
                if (nextChar == '\n')    // TODO: error
                {
                    LineNumber++;
                    ColumnNumber = 0;
                    break;
                }
                if (nextChar == '\\')
                {
                    readNextChar();
                    switch (nextChar)
                    {
                        case 'n': buffer.Append('\n'); break;
                        case 't': buffer.Append('\t'); break;
                        case '\"': buffer.Append('\"'); break;
                        case '\\': buffer.Append('\\'); break;
                        default:    // TODO: error
                            buffer.Append('\\');
                            buffer.Append((char)nextChar);
                            break;
                    }
                }
                else
                    buffer.Append((char)nextChar);
                readNextChar();
            }
            StringValue = buffer.ToString();
            readNextChar();
            return Token.String;
        }

        private void readNextChar()
        {
            nextChar = reader.Read();
            if (nextChar > 0)
                ColumnNumber++;
        }

        // TODO: give some limit
    }
}
