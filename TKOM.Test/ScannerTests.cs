using System.IO;
using Xunit;

namespace TKOM.Test
{
    public class ScannerTests
    {
        private Scanner buildScanner(string program)
        {
            StringReader reader = new(program);
            return new(reader);
        }


        [Theory]
        [InlineData("")]            // nothing
        [InlineData(" \n\t \r\n ")] // whitespace characters
        public void EmptyProgram(string program)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.False(moved);
            Assert.Equal(Token.Error, scanner.Current);
        }

        [Theory]
        [InlineData("a", "a")]          // single letter
        [InlineData("abcd", "abcd")]    // longer identifier
        [InlineData("voId", "voId")]    // keyword with upper letter
        public void Identifier(string program, string expectedIdentifier)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.Identifier, scanner.Current);
            Assert.Equal(expectedIdentifier, scanner.strValue);

            Assert.False(scanner.MoveNext());
        }

        [Theory]
        [InlineData("1", 1)]            // single digit
        [InlineData("10432", 10432)]    // more digits
        [InlineData("007", 7)]          // leading zeros
        public void IntConst(string program, int expectedValue)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.IntConst, scanner.Current);
            Assert.Equal(expectedValue, scanner.intValue);

            Assert.False(scanner.MoveNext());
        }

        [Theory]
        // Keywords
        [InlineData("void", Token.Void)]    [InlineData("int", Token.Int)] 
        [InlineData("return", Token.Return)]
        [InlineData("if", Token.If)]        [InlineData("else", Token.Else)]    [InlineData("while", Token.While)]
        [InlineData("read", Token.Read)]    [InlineData("print", Token.Print)]
        [InlineData("try", Token.Try)]      [InlineData("catch", Token.Catch)]  [InlineData("finally", Token.Finally)]
        [InlineData("throw", Token.Throw)]  [InlineData("when", Token.When)]    [InlineData("Exception", Token.Exception)]
        // Operators
        [InlineData("(", Token.RoundBracketOpen)]   [InlineData(")", Token.RoundBracketClose)]
        [InlineData("{", Token.CurlyBracketOpen)]   [InlineData("}", Token.CurlyBracketClose)]
        [InlineData("-", Token.Minus)]      [InlineData("+", Token.Plus)]           [InlineData("*", Token.Star)]   [InlineData("/", Token.Slash)]
        [InlineData("||", Token.Or)]        [InlineData("&&", Token.And)]
        [InlineData("<", Token.LessThan)]   [InlineData(">", Token.GreaterThan)]    [InlineData("=", Token.Equals)] [InlineData("!", Token.Not)]
        [InlineData(";", Token.Semicolon)]  [InlineData(",", Token.Comma)]          [InlineData(".", Token.Dot)]
        public void SimpleToken(string program, Token token)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(token, scanner.Current);

            Assert.False(scanner.MoveNext());
        }

        [Theory]
        [InlineData("//", "")]                              // empty
        [InlineData("// rs 278 %^&*+=", " rs 278 %^&*+=")]  // nonempty ended with EOF
        [InlineData("//abcd\n", "abcd")]                    // ended with \n
        [InlineData("//\" abc\"", "\" abc\"")]              // comment with a string
        public void Comment(string program, string expectedValue)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.Comment, scanner.Current);
            Assert.Equal(expectedValue, scanner.strValue);

            Assert.False(scanner.MoveNext());
        }

        [Theory]
        [InlineData("\"\"", "")]                                    // empty
        [InlineData("\" asb 78 @#$%^&* l\"", " asb 78 @#$%^&* l")]  // nonempty
        [InlineData("\"// abc\"", "// abc")]                        // string with a comment
        [InlineData("\"12 \\\" 34", "12 \" 34")]                    // special character - quote
        [InlineData("\"12 \\n 34", "12 \n 34")]                     // special character - newline
        [InlineData("\"12 \\t 34", "12 \t 34")]                     // special character - tabulator
        [InlineData("\"12 \\\\ 34", "12 \\ 34")]                    // special character - slash
        [InlineData("\"12 \\x 34", "12 \\x 34")]                    // illegal special character
        [InlineData("\"abcd", "abcd")]                              // ended with an EOF
        public void String(string program, string expectedValue)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.String, scanner.Current);
            Assert.Equal(expectedValue, scanner.strValue);

            Assert.False(scanner.MoveNext());
        }

        [Theory]
        [InlineData("|")] [InlineData("&")]
        [InlineData("\\")]  // backslash is not a divide operator
        public void ErrorToken(string program)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.Error, scanner.Current);
            
            Assert.False(scanner.MoveNext());
        }

        [Theory]
        [InlineData("vx abc", new[] { Token.Identifier, Token.Identifier }, new[] { "vx", "abc" })]     // identifier starting as keyword
        [InlineData("void abc void", new[] { Token.Void, Token.Identifier, Token.Void }, new[] { null, "abc", null })]  // multiple tokens
        [InlineData("&A", new[] { Token.Error, Token.Identifier }, new[] { null, "A" })]                // only first char of AND operator
        [InlineData("&&Exception", new[] { Token.And, Token.Exception })]                               // after AND operator
        [InlineData("|void", new[] { Token.Error, Token.Void })]                                        // only first char of OR operator
        [InlineData("||int", new[] { Token.Or, Token.Int })]                                            // after OR operator
        [InlineData("/8", new[] { Token.Slash, Token.IntConst }, new object[] { null, 8 })]             // after short operator
        [InlineData("+41", new[] { Token.Plus, Token.IntConst }, new object[] { null, 41 })]            // number with plus
        [InlineData("-44", new[] { Token.Minus, Token.IntConst }, new object[] { null, 44 })]           // number with minus
        [InlineData("2+2", new[] { Token.IntConst, Token.Plus, Token.IntConst }, new object[] { 2, null, 2 })]  // simple equation
        [InlineData("\"AA\n BB", new[] { Token.String, Token.Identifier }, new[] { "AA", "BB" })]       // string broken by newline
        public void Program(string program, Token[] tokens, object[] values = null)
        {
            Scanner scanner = buildScanner(program);

            for (int i = 0; i < tokens.Length; i++)
            {
                Assert.True(scanner.MoveNext());
                Assert.Equal(tokens[i], scanner.Current);
                if (values is not null)
                    if (values[i] is string)
                        Assert.Equal(values[i], scanner.strValue);
                    else if (values[i] is int)
                        Assert.Equal(values[i], scanner.intValue);
            }
            Assert.False(scanner.MoveNext());
        }
    }
}
