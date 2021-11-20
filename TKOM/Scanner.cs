﻿using System.IO;
using System.Text;

namespace TKOM
{
    public enum Token
    {
        Error,
        Identifier,         // [a-zA-Z][a-zA-Z0-9]*
        IntConst,           // 0|([1-9][0-9]*)
        // Keywords
        Void, Int,
        Return,
        If, Else, While,
        Read, Print,
        Try, Catch, Finally, Throw, When, Exception,
        // Operators
        RoundBracketOpen, RoundBracketClose,
        CurlyBracketOpen, CurlyBracketClose,
        Minus, Plus, Star, Slash,
        Or, And,
        LessThan, GreaterThan, Equals, Not,
        Semicolon, Comma, Dot
    }

    public class Scanner
    {
        private readonly TextReader reader;
        public Token Current;
        
        private int nextChar;
        public string strValue;
        public int intValue;

        public Scanner(TextReader reader)
        {
            this.reader = reader;
            Current = Token.Error;
            nextChar = reader.Read();
        }

        public bool MoveNext()
        {
            skipWhitespaces();
            char ch = (char)nextChar;

            StringBuilder buffer = new();
            buffer.Append(ch);
            if (char.IsLetter(ch))
            {
                readWhileLetterOrDigit(buffer);
                strValue = buffer.ToString();
                Current = strValue switch
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
                intValue = int.Parse(buffer.ToString());
                Current = Token.IntConst;
            }
            else if (nextChar < 0)
                return false;
            else
            {
                switch (ch)
                {
                    case '|': Current = tryReadOrToken(); break;
                    case '&': Current = tryReadAndToken(); break;
                    default:  Current = ch switch
                        {
                            '(' => Token.RoundBracketOpen,
                            ')' => Token.RoundBracketClose,
                            '{' => Token.CurlyBracketOpen,
                            '}' => Token.CurlyBracketClose,
                            '-' => Token.Minus,
                            '+' => Token.Plus,
                            '*' => Token.Star,
                            '/' => Token.Slash,
                            '<' => Token.LessThan,
                            '>' => Token.GreaterThan,
                            '=' => Token.Equals,
                            '!' => Token.Not,
                            ';' => Token.Semicolon,
                            ',' => Token.Comma,
                            '.' => Token.Dot,
                            _ => Token.Error
                        };
                        nextChar = reader.Read();
                        break;
                }
            }
            return true;
        }

        private void skipWhitespaces()
        {
            while (char.IsWhiteSpace((char)nextChar))
                nextChar = reader.Read();
        }

        private void readWhileLetterOrDigit(StringBuilder buffer)
        {
            nextChar = reader.Read();
            char ch = (char)nextChar;
            while (char.IsLetterOrDigit(ch))
            {
                buffer.Append(ch);
                nextChar = reader.Read();
                ch = (char)nextChar;
            }
        }

        private void readWhileDigit(StringBuilder buffer)
        {
            nextChar = reader.Read();
            char ch = (char)nextChar;
            while (char.IsDigit(ch))
            {
                buffer.Append(ch);
                nextChar = reader.Read();
                ch = (char)nextChar;
            }
        }

        private Token tryReadOrToken()
        {
            nextChar = reader.Read();
            switch (nextChar)
            {
                case '|':
                    nextChar = reader.Read();
                    return Token.Or;
                default:
                    return Token.Error;
            }
        }

        private Token tryReadAndToken()
        {
            nextChar = reader.Read();
            switch (nextChar)
            {
                case '&':
                    nextChar = reader.Read();
                    return Token.And;
                default:
                    return Token.Error;
            }
        }

        // TODO: leading zeros error
        // TODO: give some limit
    }
}
