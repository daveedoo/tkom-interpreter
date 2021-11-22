using Xunit;

namespace TKOM.Scanner.Test
{
    public partial class ScannerTests
    {
        [Fact]
        public void Line_InitializedAsOne()
        {
            IScanner scanner = buildScanner("anything");

            Assert.Equal<uint>(1, scanner.LineNumber);
        }

        [Fact]
        public void Column_InitializedAsZero()
        {
            IScanner scanner = buildScanner("anything");

            Assert.Equal<uint>(1, scanner.ColumnNumber);
        }

        [Fact]
        public void skipWhitespaces_CountsNewlines()
        {
            IScanner scanner = buildScanner("  \n \t\t \n ");

            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.LineNumber);
        }

        [Fact]
        public void readStringToken_CountsNewlines()
        {
            IScanner scanner = buildScanner("\"some string with \n newline inside\"");

            scanner.MoveNext();
            scanner.MoveNext();

            Assert.Equal<uint>((uint)2, scanner.LineNumber);
        }

        [Fact]
        public void skipWhitespaces_AdvancesColumnNumber()
        {
            IScanner scanner = buildScanner("\n \t ");

            scanner.MoveNext();

            Assert.Equal<uint>(4, scanner.ColumnNumber);
        }

        [Fact]
        public void skipWhitespaces_ResetsColumnNumberEveryNewline()
        {
            IScanner scanner = buildScanner("\n \t \n  ");

            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.ColumnNumber);
        }

        [Fact]
        public void readStringToken_AdvancesColumnNumber()
        {
            IScanner scanner = buildScanner("abc");

            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.ColumnNumber);
        }

        [Fact]
        public void readStringToken_ResetsColumnNumberEveryNewline()
        {
            IScanner scanner = buildScanner("\"abc\n xyz\"");

            scanner.MoveNext();

            Assert.Equal<uint>(1, scanner.ColumnNumber);
        }

        [Fact]
        public void WhenMovingAfterEOF_DoesntAdvanceColumnNumber()
        {
            IScanner scanner = buildScanner("abc");

            scanner.MoveNext();
            scanner.MoveNext();

            Assert.Equal<uint>(4, scanner.ColumnNumber);
        }

    }
}
