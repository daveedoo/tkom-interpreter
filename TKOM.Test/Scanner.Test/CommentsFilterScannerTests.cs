using System.IO;
using TKOM.ErrorHandler;
using Xunit;

namespace TKOM.Scanner.Test
{
    public class CommentsFilterScannerTests
    {
        [Theory]
        [InlineData("xyz //comment\n17")]
        [InlineData("xyz //comment1\n//comment2\n//comment3\n17")]
        public void SkipsComment(string program)
        {
            StringReader reader = new StringReader(program);
            IErrorHandler errorHandler = new ErrorCollecter();
            Scanner baseScanner = new Scanner(reader, errorHandler);
            IScanner scanner = new CommentsFilterScanner(baseScanner);

            Assert.True(scanner.MoveNext());
            Assert.Equal(Token.Identifier, scanner.Current);
            Assert.Equal("xyz", scanner.StringValue);
            
            Assert.True(scanner.MoveNext());
            Assert.Equal(Token.IntConst, scanner.Current);
            Assert.Equal(17, scanner.IntValue);

            Assert.False(scanner.MoveNext());
        }
    }
}
