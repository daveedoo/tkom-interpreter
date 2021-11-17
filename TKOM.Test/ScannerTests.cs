using System;
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
        [InlineData("")]
        [InlineData(" \n\t \r\n ")]
        public void EmptyProgram(string program)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.False(moved);
            Assert.Equal(Token.Error, scanner.Current);
        }

        [Theory]
        [InlineData("a", "a")]
        [InlineData("abcd", "abcd")]
        [InlineData("voId", "voId")]
        public void Identifier(string program, string expectedIdentifier)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.Identifier, scanner.Current);
            Assert.Equal(expectedIdentifier, scanner.strValue);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("10", 10)]
        //[InlineData("007", 7)]
        public void IntConst(string program, int expectedValue)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(Token.IntConst, scanner.Current);
            Assert.Equal(expectedValue, scanner.intValue);
        }

        [Theory]
        [InlineData("void", Token.Void)]
        public void Keyword(string program, Token token)
        {
            Scanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(token, scanner.Current);
        }

        [Theory]
        [InlineData("vx abc", new[] {Token.Identifier, Token.Identifier}, new[] { "vx", "abc" })]
        [InlineData("void abc void", new[] { Token.Void, Token.Identifier, Token.Void }, new[] { null, "abc", null })]
        public void Program(string program, Token[] tokens, string[] values)
        {
            Scanner scanner = buildScanner(program);

            for (int i = 0; i < tokens.Length; i++)
            {
                Assert.True(scanner.MoveNext());
                Assert.Equal(tokens[i], scanner.Current);
                if (values[i] is not null)
                    Assert.Equal(values[i], scanner.strValue);
            }
        }
    }
}
