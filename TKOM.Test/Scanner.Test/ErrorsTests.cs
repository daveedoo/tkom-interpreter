using System;
using System.IO;
using TKOM.ErrorHandler;
using Xunit;

namespace TKOM.Scanner.Test
{
    public partial class ScannerTests
    {
        [Fact]
        public void VeryLongIdentifier_ThrowsErrorToken()
        {
            string many_a = new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            StringReader reader = new StringReader(many_a);
            ErrorCollecter errorCollecter = new ErrorCollecter();
            Scanner scanner = new Scanner(reader, errorCollecter);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
            Assert.Equal(Token.Error, scanner.Current);
        }

        [Fact]
        public void IntMaxValue_IsCorrect()
        {
            IScanner scanner = buildScanner(int.MaxValue.ToString());

            scanner.MoveNext();
            int value = scanner.IntValue;

            Assert.Equal(int.MaxValue, value);
        }

        [Fact]
        public void OverIntMaxValue_ThrowsErrorToken()
        {
            long maxValuePlusOne = (long)int.MaxValue + 1;
            string program = maxValuePlusOne.ToString();
            TextReader reader = new StringReader(program);
            ErrorCollecter errorCollecter = new ErrorCollecter();
            IScanner scanner = new Scanner(reader, errorCollecter);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
            Assert.Equal(Token.Error, scanner.Current);
        }

        [Fact]
        public void VeryLongNumber_ThrowsErrorToken()
        {
            string many_1s = new string('1', 2 * Scanner.MAX_TOKEN_LENGTH + 1);
            StringReader reader = new StringReader(many_1s);
            ErrorCollecter errorCollecter = new ErrorCollecter();
            Scanner scanner = new Scanner(reader, errorCollecter);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
            Assert.Equal(Token.Error, scanner.Current);
        }

        [Fact]
        public void VeryLongComment_ThrowsWarning()
        {
            string many_a = "//" + new string('a', Scanner.MAX_TOKEN_LENGTH + 1);
            StringReader reader = new StringReader(many_a);
            ErrorCollecter errorCollecter = new ErrorCollecter();
            Scanner scanner = new Scanner(reader, errorCollecter);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.warningsCount);
            Assert.Equal(Token.Comment, scanner.Current);
        }

        [Fact]
        public void VeryLongComment_IsCut()
        {
            string many_a = "//" + new string('a', 2*Scanner.MAX_TOKEN_LENGTH + 1);
            StringReader reader = new StringReader(many_a);
            ErrorCollecter errorCollecter = new ErrorCollecter();
            Scanner scanner = new Scanner(reader, errorCollecter);

            scanner.MoveNext();

            Assert.Equal(scanner.StringValue, new string('a', Scanner.MAX_TOKEN_LENGTH));
        }

        [Fact]
        public void VeryLongString_ThrowsErrorToken()
        {
            string many_a = "\"" + new string('a', 2 * Scanner.MAX_TOKEN_LENGTH + 1) + "\"";
            StringReader reader = new StringReader(many_a);
            ErrorCollecter errorCollecter = new ErrorCollecter();
            Scanner scanner = new Scanner(reader, errorCollecter);

            scanner.MoveNext();

            Assert.Equal(1, errorCollecter.errorCount);
            Assert.Equal(Token.Error, scanner.Current);
        }
    }
}
