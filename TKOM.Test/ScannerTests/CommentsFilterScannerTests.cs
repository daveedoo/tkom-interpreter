using System.IO;
using TKOM.ErrorHandler;
using TKOM.Scanner;
using Xunit;

namespace TKOMTest.ScannerTests
{
    public class CommentsFilterScannerTests
    {
        [Theory]
        [InlineData("xyz //comment\n17")]
        [InlineData("xyz //comment1\n//comment2\n//comment3\n17")]
        public void SkipsComment(string program)
        {
            StringReader reader = new StringReader(program);
            IErrorHandler errorHandler = new ErrorsCollector();
            Scanner baseScanner = new(reader, errorHandler);
            IScanner scanner = new TKOM.Scanner.CommentsFilterScanner(baseScanner);

            Assert.True(scanner.MoveNext());
            Assert.Equal(Token.Identifier, scanner.Current);
            Assert.Equal("xyz", scanner.StringValue);
            
            Assert.True(scanner.MoveNext());
            Assert.Equal(Token.IntConst, scanner.Current);
            Assert.Equal(17, scanner.IntValue);

            Assert.True(scanner.MoveNext());
            Assert.Equal(Token.EOF, scanner.Current);
            Assert.False(scanner.MoveNext());
        }
    }
}
