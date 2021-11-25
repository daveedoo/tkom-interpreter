using System;
using System.IO;
using TKOM.ErrorHandler;
using Xunit;

namespace TKOM.Scanner.Test
{
    public partial class ScannerTests
    {
        [Fact]
        public void WhenAlmostVeryLongIdentifier_BehavesNormally()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH);
            IScanner scanner = buildScanner(many_a);

            scanner.MoveNext();

            Assert.Equal(Token.Identifier, scanner.Current);
            Assert.Equal(many_a, scanner.StringValue);
        }
        [Fact]
        public void WhenAlmostVeryLongIdentifier_DoesNotThrowError()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH);
            IScanner scanner = buildScanner(many_a);
            
            scanner.MoveNext();

            Assert.Equal(0, errorCollecter.errorCount);
        }
        [Fact]
        public void WhenVeryLongIdentifier_BehavesAsIfEOF()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_a);

            bool moved = scanner.MoveNext();    // TODO: add check for next move is false
            
            Assert.False(moved);
            Assert.Equal(Token.Error, scanner.Current);
        }
        [Fact]
        public void WhenVeryLongIdentifier_ThrowsError()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_a);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
        }


        [Fact]
        public void WhenIntMaxValue_BehavesNormally()
        {
            IScanner scanner = buildScanner(int.MaxValue.ToString());

            scanner.MoveNext();

            Assert.Equal(Token.IntConst, scanner.Current);
            Assert.Equal(int.MaxValue, scanner.IntValue);
        }
        [Fact]
        public void WhenOverIntMaxValue_BehavesAsIfEOF()
        {
            long maxValuePlusOne = (long)int.MaxValue + 1;
            string program = maxValuePlusOne.ToString();
            IScanner scanner = buildScanner(program);

            bool moved = scanner.MoveNext();

            Assert.False(moved);
            Assert.Equal(Token.Error, scanner.Current);
        }
        [Fact]
        public void WhenOverIntMaxValue_ThrowsError()
        {
            long maxValuePlusOne = (long)int.MaxValue + 1;
            string program = maxValuePlusOne.ToString();
            IScanner scanner = buildScanner(program);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
        }
        [Fact]
        public void WhenVeryLongNumber_BehavesAsIfEOF()
        {
            string many_1s = new string('1', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_1s);

            bool moved = scanner.MoveNext();

            Assert.False(moved);
            Assert.Equal(Token.Error, scanner.Current);
        }
        [Fact]
        public void WhenVeryLongNumber_ThrowsError()
        {
            string many_1s = new string('1', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_1s);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
        }

        [Fact]
        public void WhenVeryLongComment_ThrowsWarning()
        {
            string many_a = "//" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            IScanner scanner = buildScanner(many_a);

            scanner.MoveNext();

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
        public void VeryLongString_ThrowsErrorToken()
        {
            string many_a_string = "\"" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1) + "\"";
            IScanner scanner = buildScanner(many_a_string);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
            Assert.Equal(Token.Error, scanner.Current);
        }
    }
}
