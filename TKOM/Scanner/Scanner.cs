using System.IO;
using System.Text;
using TKOM.ErrorHandler;

namespace TKOM.Scanner
{
    public class Scanner : IScanner
    {
        public static readonly int MAX_TOKEN_LENGTH = 500;

        private IErrorHandler errorHandler;
        private readonly TextReader reader;
        public Token Current { get; private set; }

        private bool eof = false;
        private int nextChar;
        public string StringValue { get; private set; }
        public int IntValue { get; private set; }

        public uint LineNumber { get; private set; }
        public uint ColumnNumber { get; private set; }

        public Scanner(TextReader reader, IErrorHandler errorHandler)
        {
            this.errorHandler = errorHandler;
            this.reader = reader;
            Current = Token.Error;
            LineNumber = 1;
            ColumnNumber = 0;
            readNextChar();
        }

        public bool MoveNext()
        {
            skipWhitespaces();
            char ch = (char)nextChar;

            StringBuilder buffer = new();
            buffer.Append(ch);
            if (char.IsLetter(ch))
            {
                if (!readWhileLetterOrDigit(buffer))
                {
                    Current = Token.Error;
                    return true;
                }
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
                (uint startLine, uint startColumn) errStart = (LineNumber, ColumnNumber);
                if (!readWhileDigit(buffer))
                {
                    Current = Token.Error;
                    return true;
                }
                if (int.TryParse(buffer.ToString(), out int value))
                {
                    Current = Token.IntConst;
                    IntValue = value;
                }
                else
                {
                    LexLocation location = new LexLocation(errStart.startLine, errStart.startColumn, LineNumber, ColumnNumber);
                    errorHandler.HandleError(location, "Integral constant is too large");
                    Current = Token.Error;
                }
            }
            else if (nextChar < 0)
            {
                if (!eof)
                {
                    ColumnNumber++;
                    eof = true;
                }
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

        private bool readWhileLetterOrDigit(StringBuilder buffer)
        {
            (uint startLine, uint startColumn) errStart = (LineNumber, ColumnNumber);
            readNextChar();
            char ch = (char)nextChar;
            while (char.IsLetterOrDigit(ch) && buffer.Length < MAX_TOKEN_LENGTH)
            {
                buffer.Append(ch);
                readNextChar();
                ch = (char)nextChar;
            }
            if (char.IsLetterOrDigit((char)nextChar))
            {
                do
                    readNextChar();
                while (char.IsLetterOrDigit((char)nextChar)) ;
                LexLocation location = new LexLocation(errStart.startLine, errStart.startColumn, LineNumber, ColumnNumber);
                errorHandler.HandleError(location, "Identifier is too long");
                return false;
            }
            return true;
        }

        private bool readWhileDigit(StringBuilder buffer)
        {
            (uint startLine, uint startColumn) errStart = (LineNumber, ColumnNumber);
            readNextChar();
            char ch = (char)nextChar;
            while (char.IsDigit(ch) && buffer.Length < MAX_TOKEN_LENGTH)
            {
                buffer.Append(ch);
                readNextChar();
                ch = (char)nextChar;
            }
            if (char.IsDigit((char)nextChar))
            {
                do
                    readNextChar();
                while (char.IsDigit((char)nextChar)) ;
                LexLocation location = new LexLocation(errStart.startLine, errStart.startColumn, LineNumber, ColumnNumber);
                errorHandler.HandleError(location, "Number is too long");
                return false;
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
                (uint startLine, uint startColumn) errStart = (LineNumber, ColumnNumber);
                StringBuilder buffer = new();
                readNextChar();
                while (nextChar >= 0 && nextChar != '\n' && buffer.Length < MAX_TOKEN_LENGTH)
                {
                    buffer.Append((char)nextChar);
                    readNextChar();
                }
                if (buffer.Length >= MAX_TOKEN_LENGTH)
                {
                    do
                        readNextChar();
                    while (nextChar >= 0 && nextChar != '\n');
                    LexLocation location = new LexLocation(errStart.startLine, errStart.startColumn, LineNumber, ColumnNumber);
                    errorHandler.HandleWarning(location, "This comment is very long!");
                }
                StringValue = buffer.ToString();
                return Token.Comment;
            }
            return Token.Slash;
        }

        private Token readStringToken()
        {
            (uint startLine, uint startColumn) errStart = (LineNumber, ColumnNumber);
            StringBuilder buffer = new();
            readNextChar();
            while (nextChar >= 0 && nextChar != '"' && buffer.Length < MAX_TOKEN_LENGTH - 1)
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
            if (buffer.Length >= MAX_TOKEN_LENGTH - 1)
            {
                do
                    readNextChar();
                while (nextChar >= 0 && nextChar != '"');
                LexLocation location = new LexLocation(errStart.startLine, errStart.startColumn, LineNumber, ColumnNumber);
                errorHandler.HandleError(location, "String is too long");
                readNextChar();
                return Token.Error;
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
    }
}
