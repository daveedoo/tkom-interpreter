using Xunit;

namespace TKOM.Scanner.Test
{
    // actually there is probably too many tests
    // this class should be testing PositionTrackingTextReader class, TODO in unspecified future
    public partial class ScannerTests
    {
        [Fact]
        public void Line_InitializedAsOne()
        {
            IScanner scanner = buildScanner("anything");

            Assert.Equal<uint>(1, scanner.Position.Line);
        }

        [Fact]
        public void Column_InitializedAsZero()
        {
            IScanner scanner = buildScanner("anything");

            Assert.Equal<uint>(0, scanner.Position.Column);
        }

        [Fact]
        public void skipWhitespaces_CountsNewlines()
        {
            IScanner scanner = buildScanner("  \n \t\t \n ");

            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.Position.Line);
        }

        [Fact]
        public void readStringToken_CountsNewlines()
        {
            IScanner scanner = buildScanner("\"some string with \n newline inside\"");

            scanner.MoveNext();
            scanner.MoveNext();

            Assert.Equal<uint>(2, scanner.Position.Line);
        }

        [Fact]
        public void skipWhitespaces_AdvancesColumnNumber()
        {
            IScanner scanner = buildScanner("\n \t ");

            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.Position.Column);
        }

        [Fact]
        public void skipWhitespaces_ResetsColumnNumberEveryNewline()
        {
            IScanner scanner = buildScanner("\n \t \n  ");

            scanner.MoveNext();

            Assert.Equal<uint>(2, scanner.Position.Column);
        }

        [Fact]
        public void readStringToken_AdvancesColumnNumber()
        {
            IScanner scanner = buildScanner("abc");

            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.Position.Column);
        }

        [Fact]
        public void readStringToken_ResetsColumnNumberEveryNewline()
        {
            IScanner scanner = buildScanner("\"abc\n xyz\"");

            scanner.MoveNext();

            Assert.Equal<uint>(4, scanner.Position.Column);
        }

        [Fact]
        public void WhenMovingAfterEOF_DoesntAdvanceColumnNumber()
        {
            IScanner scanner = buildScanner("abc");

            scanner.MoveNext();
            scanner.MoveNext();

            Assert.Equal<uint>(3, scanner.Position.Column);
        }
    }
}
