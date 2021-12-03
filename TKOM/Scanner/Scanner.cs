using System.IO;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public class Scanner : IScanner
    {
        public static readonly int MAX_TOKEN_LENGTH = 500;

        public IErrorHandler ErrorHandler { get; }
        public Token Current { get; private set; }

        public string StringValue { get; private set; }
        public int IntValue { get; private set; }

        private readonly PositionTrackingTextReader reader;
        public Position Position => reader.Position;


        public Scanner(TextReader reader, IErrorHandler errorHandler)
        {
            this.ErrorHandler = errorHandler;
            this.reader = new PositionTrackingTextReader(reader);
            Current = Token.Error;
            buffer = new LimitedStringBuilder(MAX_TOKEN_LENGTH);
            //this.reader.ReadNext();
        }

        private Position tokenStartPosition;
        private LimitedStringBuilder buffer;

        public bool MoveNext()
        {
            skipWhitespaces();
            tokenStartPosition = new Position(Position.Line, Position.Column);

            buffer.Clear();
            if (char.IsLetter(reader.NextChar))
                return tryReadKeywordOrIdentifier();
            else if (char.IsDigit(reader.NextChar))
                return tryReadIntConst();
            else if (reader.eof)
                return false;
            return tryReadSymbolStartingToken();
        }

        private void skipWhitespaces()
        {
            while (char.IsWhiteSpace(reader.NextChar))
                reader.Move();
        }

        private bool tryReadKeywordOrIdentifier()
        {
            while (char.IsLetterOrDigit(reader.NextChar))
            {
                if (!buffer.Append(reader.NextChar))
                {
                    throwError("Buffer overflow. Too long identifier.");
                    return false;   // TODO: tidy-up
                }
                reader.Move();
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
            while (char.IsDigit(reader.NextChar))
            {
                buffer.Append(reader.NextChar);
                reader.Move();
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
            switch (reader.NextChar)
            {
                case '|': Current = tryReadOrToken(); break;
                case '&': Current = tryReadAndToken(); break;
                case '/': Current = tryReadCommentToken(); break;
                case '"': Current = readStringToken(); break;
                default:
                    Current = reader.NextChar switch
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
                    reader.Move();
                    break;
            }
            return true;
        }

        private Token tryReadOrToken()
        {
            reader.Move();
            switch (reader.NextChar)
            {
                case '|':
                    reader.Move();
                    return Token.Or;
                default:
                    return Token.Error;
            }
        }

        private Token tryReadAndToken()
        {
            reader.Move();
            switch (reader.NextChar)
            {
                case '&':
                    reader.Move();
                    return Token.And;
                default:
                    return Token.Error;
            }
        }

        private Token tryReadCommentToken()
        {
            reader.Move();
            if (reader.NextChar == '/')
            {
                while (reader.Move() && reader.NextChar != '\n')
                {
                    if (!buffer.Append(reader.NextChar))
                    {
                        throwWarning();
                        while (reader.Move() && reader.NextChar != '\n')     // ???????????
                            ;
                            //reader.ReadNext();
                    }
                }
                StringValue = buffer.ToString();
                return Token.Comment;
            }
            return Token.Slash;
        }

        private Token readStringToken()
        {
            while (reader.Move() && reader.NextChar != '"' && reader.NextChar != '\n')        // po co w ogóle robić ograniczenia, jeśli trudno ograniczyć stringa?
            {
                bool appended;
                if (reader.NextChar == '\\')
                {
                    reader.Move();
                    switch (reader.NextChar)
                    {
                        case 'n': appended = buffer.Append('\n'); break;
                        case 't': appended = buffer.Append('\t'); break;
                        case '\"': appended = buffer.Append('\"'); break;
                        case '\\': appended = buffer.Append('\\'); break;
                        default:    // TODO: error
                            if (buffer.Append('\\'))
                                appended = buffer.Append(reader.NextChar);
                            else appended = false;
                            break;
                    }
                }
                else
                    appended = buffer.Append(reader.NextChar);
                if (!appended)
                {
                    throwError("Buffer overflow. Too long string.");
                    return Token.Error;
                }
            }

            if (reader.NextChar == '\n')    // TODO: error
            {
                // TODO: inne newline'y
                StringValue = buffer.ToString();
                return Token.String;
            }
            else
            {
                StringValue = buffer.ToString();
                reader.Move();  // read second quote (")
                return Token.String;
            }
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
