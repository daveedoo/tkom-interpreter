using System.IO;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public class Scanner : IScanner
    {
        public static readonly int MAX_TOKEN_LENGTH = 500;

        public IErrorHandler ErrorHandler { get; }
        private readonly TextReader reader;
        public Token Current { get; private set; }

        public string StringValue { get; private set; }
        public int IntValue { get; private set; }

        public Position Position { get; private set; }

        public Scanner(TextReader reader, IErrorHandler errorHandler)
        {
            this.ErrorHandler = errorHandler;
            this.reader = reader;
            Current = Token.Error;
            Position = new Position(1, 0);
            buffer = new LimitedStringBuilder(MAX_TOKEN_LENGTH);
            readNextChar();
        }

        private Position tokenStartPosition;
        private LimitedStringBuilder buffer;
        private bool eof = false;
        private int nextChar;

        public bool MoveNext()
        {
            skipWhitespaces();
            tokenStartPosition = new Position(Position.Line, Position.Column);

            buffer.Clear();
            char ch = (char)nextChar;
            if (char.IsLetter(ch))
                return tryReadKeywordOrIdentifier();
            else if (char.IsDigit(ch))
                return tryReadIntConst();
            else if (nextChar < 0)
                return false;
            return tryReadSymbolStartingToken();
        }

        private void skipWhitespaces()
        {
            while (char.IsWhiteSpace((char)nextChar))
                readNextChar();
        }

        private bool tryReadKeywordOrIdentifier()
        {
            char ch = (char)nextChar;
            while (char.IsLetterOrDigit(ch))
            {
                if (!buffer.Append(ch))
                {
                    throwError("Buffer overflow. Too long identifier.");
                    return false;   // TODO: tidy-up
                }
                readNextChar();
                ch = (char)nextChar;
            }
            StringValue = buffer.ToString();
            Current = StringValue switch
            {
                "void" => Token.Void,
                "int" => Token.Int,
                "return" => Token.Return,
                "if" => Token.If,
                "else" => Token.Else,
                "while" => Token.While,
                "read" => Token.Read,
                "print" => Token.Print,
                "try" => Token.Try,
                "catch" => Token.Catch,
                "finally" => Token.Finally,
                "throw" => Token.Throw,
                "when" => Token.When,
                "Exception" => Token.Exception,
                _ => Token.Identifier
            };
            return true;
        }

        private bool tryReadIntConst()
        {
            char ch = (char)nextChar;
            while (char.IsDigit(ch))
            {
                buffer.Append(ch);
                readNextChar();
                ch = (char)nextChar;
            }

            if (int.TryParse(buffer.ToString(), out int value)) // TODO: poprawić
            {
                Current = Token.IntConst;
                IntValue = value;
            }
            else
            {
                throwError("Buffer overflow. Too long integral constant.");
                return false;
            }
            return true;
        }

        private bool tryReadSymbolStartingToken()
        {
            char ch = (char)nextChar;
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
                        '<' => Token.LessThan,  // dwuznaki
                        '>' => Token.GreaterThan,
                        '=' => Token.Equals,    // ==
                        '!' => Token.Not,
                        ';' => Token.Semicolon,
                        ',' => Token.Comma,
                        '.' => Token.Dot,
                        _ => Token.Error
                    };
                    readNextChar();
                    break;
            }
            return true;
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
                readNextChar();
                while (nextChar >= 0 && nextChar != '\n')
                {
                    if (!buffer.Append((char)nextChar))
                    {
                        throwWarning();
                        while (nextChar >= 0 && nextChar != '\n')
                            readNextChar();
                    }
                    readNextChar();
                }
                StringValue = buffer.ToString();
                return Token.Comment;
            }
            return Token.Slash;
        }

        private Token readStringToken()
        {
            LimitedStringBuilder buffer = new(MAX_TOKEN_LENGTH);
            readNextChar();
            while (nextChar >= 0 && nextChar != '"' && nextChar != '\n')        // po co w ogóle robić ograniczenia, jeśli trudno ograniczyć stringa?
            {
                bool appended;
                if (nextChar == '\\')
                {
                    readNextChar();
                    switch (nextChar)
                    {
                        case 'n': appended = buffer.Append('\n'); break;
                        case 't': appended = buffer.Append('\t'); break;
                        case '\"': appended = buffer.Append('\"'); break;
                        case '\\': appended = buffer.Append('\\'); break;
                        default:    // TODO: error
                            if (buffer.Append('\\'))
                                appended = buffer.Append((char)nextChar);
                            else appended = false;
                            break;
                    }
                }
                else
                    appended = buffer.Append((char)nextChar);
                if (!appended)
                {
                    throwError("Buffer overflow. Too long string.");
                    return Token.Error;
                }
                readNextChar();
            }

            if (nextChar == '\n')    // TODO: error
            {
                // TODO: inne newline'y
                StringValue = buffer.ToString();
                return Token.String;
            }
            else
            {
                StringValue = buffer.ToString();
                readNextChar();     // read second quote (")
                return Token.String;
            }
        }

        private void readNextChar()
        {
            if (eof)
                return;

            nextChar = reader.Read();
            if (nextChar == '\n')
            {
                Position.IncrementLine();
            }
            else if (nextChar < 0)
            {
                //ColumnNumber++;
                eof = true;
            }
            else
                Position.IncrementColumn();
        }

        private void throwError(string message)
        {
            LexLocation location = new LexLocation(tokenStartPosition, Position);
            ErrorHandler.HandleError(location, message);
            Current = Token.Error;
        }
        private void throwWarning()
        {
            LexLocation location = new LexLocation(0, 0, 0, 0); // TODO popraw liczby
            ErrorHandler.HandleWarning(location, "");
            Current = Token.Error;  // TODO: czy na pewno?
        }
    }
}
