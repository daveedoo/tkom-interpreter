﻿using System;
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
        public int? IntValue { get; private set; }

        private readonly PositionTrackingTextReader reader;
        public Position Position => reader.Position;


        public Scanner(TextReader reader, IErrorHandler errorHandler)
        {
            this.ErrorHandler = errorHandler;
            this.reader = new PositionTrackingTextReader(reader);
            Current = Token.Error;
            buffer = new LimitedStringBuilder(MAX_TOKEN_LENGTH);
        }

        private Position tokenStartPosition;
        private LimitedStringBuilder buffer;

        public bool MoveNext()
        {
            skipWhitespaces();
            tokenStartPosition = new Position(Position.Line, Position.Column);

            buffer.Clear();
            if (char.IsLetter(reader.NextChar))
                tryReadKeywordOrIdentifier();
            else if (char.IsDigit(reader.NextChar))
                tryReadIntConst();
            else if (reader.eof)
                return false;
            else
                tryReadSymbolStartingToken();
            buffer.Clear();
            return true;
        }

        private void skipWhitespaces()
        {
            while (char.IsWhiteSpace(reader.NextChar))
                reader.Move();
        }
        private void skipLettersAndDigits()
        {
            while (char.IsLetterOrDigit(reader.NextChar))
                reader.Move();
        }
        private void skipDigits()
        {
            while (char.IsDigit(reader.NextChar))
                reader.Move();
        }
        private void skipCurrentLine()
        {
            while (reader.NextChar != '\n' && reader.Move())
                ;
            reader.Move();
        }
        private void skipToQuoteOrNewline()
        {
            while (reader.NextChar != '\n' && reader.NextChar != '\"' && reader.Move())
                ;
            reader.Move();
        }

        private void tryReadKeywordOrIdentifier()
        {
            while (char.IsLetterOrDigit(reader.NextChar))
            {
                if (!buffer.Append(reader.NextChar))
                {
                    skipLettersAndDigits();
                    throwErrorAndClearValues("Buffer overflow. Too long identifier.");
                    Current = Token.Error;
                    return;
                }
                reader.Move();
            }
            StringValue = buffer.ToString();
            Current = StringValue switch    // TODO: dictionary
            {
                "void"      => Token.Void,
                "int"       => Token.Int,
                "return"    => Token.Return,
                "if"        => Token.If,
                "else"      => Token.Else,
                "while"     => Token.While,
                "read"      => Token.Read,
                "print"     => Token.Print,
                "try"       => Token.Try,
                "catch"     => Token.Catch,
                "finally"   => Token.Finally,
                "throw"     => Token.Throw,
                "when"      => Token.When,
                "Exception" => Token.Exception,
                _ => Token.Identifier
            };
        }

        private void tryReadIntConst()
        {
            if (reader.NextChar == '0')
            {
                reader.Move();
                if (char.IsDigit(reader.NextChar))
                {
                    skipDigits();
                    throwErrorAndClearValues("Illegal leading zero.");
                    Current = Token.Error;
                    return;
                }
                Current = Token.IntConst;
                IntValue = 0;
                return;
            }

            int value = 0;
            while (char.IsDigit(reader.NextChar))
            {
                try {
                    checked {
                        value *= 10;
                        value += reader.NextChar - '0';
                    }
                } catch (OverflowException) {
                    skipDigits();
                    throwErrorAndClearValues("Buffer overflow. Too big integral constant.");
                    Current = Token.Error;
                    return;
                } 
                reader.Move();
            }
            Current = Token.IntConst;
            IntValue = value;
        }

        private void tryReadSymbolStartingToken()
        {
            switch (reader.NextChar)
            {
                case '|': tryReadOrToken(); break;
                case '&': tryReadAndToken(); break;
                case '/': tryReadCommentToken(); break;
                case '"': tryReadStringToken(); break;
                default: tryReadSingleSymbolToken(); break;
            };
            return;
        }

        private void tryReadOrToken()
        {
            reader.Move();
            if (reader.NextChar == '|')
            {
                reader.Move();
                Current = Token.Or;
                return;
            }
            throwErrorAndClearValues("Unknown token");
            Current = Token.Error;
        }

        private void tryReadAndToken()
        {
            reader.Move();
            if (reader.NextChar == '&')
            {
                reader.Move();
                Current = Token.And;
                return;
            }
            throwErrorAndClearValues("Unknown token");
            Current = Token.Error;
        }

        private void tryReadCommentToken()
        {
            reader.Move();
            if (reader.NextChar == '/')
            {
                while (reader.Move() && reader.NextChar != '\n')
                {
                    if (!buffer.Append(reader.NextChar))
                    {
                        skipCurrentLine();
                        throwWarning("Buffer overflow. Too long comment.");
                        Current = Token.Comment;
                        break;
                    }
                }
                StringValue = buffer.ToString();
                Current = Token.Comment;
                return;
            }
            Current = Token.Slash;
        }

        private void tryReadStringToken()
        {
            while (reader.Move() && reader.NextChar != '"' && reader.NextChar != '\n')      // po co w ogóle robić ograniczenia, jeśli trudno ograniczyć stringa?
            {                                                                               // poza tym StringBuilder ma już swoje ograniczenie
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
                        default:
                            skipToQuoteOrNewline();
                            throwErrorAndClearValues("Illegal escape sequence.");
                            Current = Token.Error;
                            return;
                    }
                }
                else
                    appended = buffer.Append(reader.NextChar);
                if (!appended)
                {
                    skipToQuoteOrNewline();
                    throwErrorAndClearValues("Buffer overflow. Too long string.");
                    Current = Token.Error;
                    return;
                }
            }

            if (reader.NextChar == '\n')
            {
                throwErrorAndClearValues("String broken by newline. Close the string in current line.");
                Current = Token.Error;
            }
            else
            {
                StringValue = buffer.ToString();
                reader.Move();  // read second quote (")
                Current = Token.String;
            }
        }

        private void tryReadSingleSymbolToken()
        {
            Token t = reader.NextChar switch
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
            if (t == Token.Error)
                throwErrorAndClearValues("Unknown token");
            reader.Move();
            Current = t;
        }

        private void throwErrorAndClearValues(string message)
        {
            LexLocation location = new LexLocation(tokenStartPosition, Position);
            ErrorHandler.HandleError(location, message);

            StringValue = null;
            IntValue = null;
        }
        private void throwWarning(string message)
        {
            LexLocation location = new LexLocation(tokenStartPosition, Position);
            ErrorHandler.HandleWarning(location, message);
        }
    }
}
