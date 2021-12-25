using TKOM.Scanner;
using Xunit;

namespace TKOMTest.ScannerTests
{
    public partial class ScannerTests
    {
        public static TheoryData<string> ErrorTokenPrograms => new TheoryData<string>
        {
            "|", "&", "\\",
            new string('a', Scanner.MAX_TOKEN_LENGTH + 1),                  // very long identifier
            ((long)int.MaxValue + 1).ToString(),                            // over int.MaxValue
            "007",                                                          // leading zeros
            "\"" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1) + "\"",    // very long string
            "\"xyz\n fds\"",                                                // string broken by newline
            "\"xyz\\x\"",                                                   // illegal escape character in string
            "\"" + new string('a', Scanner.MAX_TOKEN_LENGTH) + "\\a\"",     // very long string ended with illegal escape char
        };

        [Theory]
        [MemberData(nameof(ErrorTokenPrograms))]
        public void SetsErorToken(string program)
        {
            IScanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();
            Token token = scanner.Current;

            Assert.True(moved);
            Assert.Equal(Token.Error, token);
        }

        [Theory]
        [MemberData(nameof(ErrorTokenPrograms))]
        public void ThrowsError(string program)
        {
            IScanner scanner = buildScanner(program);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
        }
        [Theory]
        [MemberData(nameof(ErrorTokenPrograms))]
        public void WhenErrorThrown_SetsValuesToNull(string program)
        {
            IScanner scanner = buildScanner($"abcd 123 {program}");

            scanner.MoveNext();
            scanner.MoveNext();
            scanner.MoveNext();
            string strVal = scanner.StringValue;
            int? intVal = scanner.IntValue;

            Assert.Null(strVal);
            Assert.Null(intVal);
        }

        // long identifiers
        [Fact]
        public void WhenAlmostVeryLongIdentifier_BehavesNormally()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH);
            IScanner scanner = buildScanner(many_a);

            scanner.MoveNext();

            Assert.Equal(Token.Identifier, scanner.Current);
            Assert.Equal(many_a, scanner.StringValue);
            Assert.Equal(0, errorCollecter.errorCount);
        }
        [Fact]
        public void WhenVeryLongIdentifier_SkipsLettersAndDigits()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_a + "xyz123&&");

            bool moved1 = scanner.MoveNext();
            Token token1 = scanner.Current;
            bool moved2 = scanner.MoveNext();
            Token token2 = scanner.Current;

            Assert.True(moved1);
            Assert.Equal(Token.Error, token1);
            Assert.True(moved2);
            Assert.Equal(Token.And, token2);
        }

        // big integers
        [Fact]
        public void WhenIntMaxValue_BehavesNormally()
        {
            IScanner scanner = buildScanner(int.MaxValue.ToString());

            scanner.MoveNext();

            Assert.Equal(Token.IntConst, scanner.Current);
            Assert.Equal(int.MaxValue, scanner.IntValue);
        }
        [Fact]
        public void WhenVeryLongNumber_SkipsDigits()
        {
            string many_1s = new string('1', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_1s + " void");

            scanner.MoveNext();
            scanner.MoveNext();
            Token token = scanner.Current;

            Assert.Equal(Token.Void, token);
        }

        // long comments
        [Fact]
        public void WhenVeryLongComment_ThrowsWarning()
        {
            string many_a = "//" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_a);

            bool moved = scanner.MoveNext();

            Assert.True(moved);
            Assert.Equal(1, errorCollecter.warningsCount);
        }
        [Fact]
        public void WhenVeryLongComment_ReturnsCommentToken()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            string program = "//" + many_a;
            IScanner scanner = buildScanner(program);

            scanner.MoveNext();

            Assert.Equal(Token.Comment, scanner.Current);
        }
        [Fact]
        public void WhenVeryLongComment_ReturnsTrimmedComment()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            string program = "//" + many_a;
            IScanner scanner = buildScanner(program);

            scanner.MoveNext();

            string max_a = new string('a', Scanner.MAX_TOKEN_LENGTH);
            Assert.Equal(max_a, scanner.StringValue);
        }
        [Fact]
        public void WhenVeryLongComment_SkipsTheLine()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            string program = $"//{many_a}\n 123";
            IScanner scanner = buildScanner(program);

            scanner.MoveNext();
            bool moved2 = scanner.MoveNext();
            Token token = scanner.Current;
            int? intVal = scanner.IntValue;

            Assert.True(moved2);
            Assert.Equal(Token.IntConst, token);
            Assert.Equal(123, intVal);
        }

        // long strings
        [Fact]
        public void WhenVeryLongString_SkipsUntilQuote()
        {
            string many_a_string = "\"" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1) + "\"";
            IScanner scanner = buildScanner($"{many_a_string} abcd");

            scanner.MoveNext();
            bool moved = scanner.MoveNext();
            Token token = scanner.Current;
            string stringVal = scanner.StringValue;

            Assert.True(moved);
            Assert.Equal(Token.Identifier, token);
            Assert.Equal("abcd", stringVal);
        }
        [Fact]
        public void WhenVeryLongString_SkipsUntilNewline()
        {
            string many_a_string = "\"" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1) + "\n";
            IScanner scanner = buildScanner($"{many_a_string} abcd");

            scanner.MoveNext();
            bool moved = scanner.MoveNext();
            Token token = scanner.Current;
            string stringVal = scanner.StringValue;

            Assert.True(moved);
            Assert.Equal(Token.Identifier, token);
            Assert.Equal("abcd", stringVal);
        }
        // invalid strings
        [Fact]
        public void WhenIllegalEscapeCharInString_SkipUntilQuote()
        {
            string program = "\"xyz\\x\" int";
            IScanner scanner = buildScanner(program);

            scanner.MoveNext();
            scanner.MoveNext();
            Token token = scanner.Current;

            Assert.Equal(Token.Int, token);
        }
    }
}
