using System.IO;
using TKOM.Parser;
using TKOM.Scanner;

namespace TKOMTest.ParserTests
{
    public abstract class ParserTests
    {
        protected ErrorCollector errorHandler;

        protected IParser buildParser(string program)
        {
            errorHandler = new ErrorCollector();
            TextReader reader = new StringReader(program);
            IScanner scanner = new Scanner(reader, errorHandler);
            return new Parser(scanner, errorHandler);
        }
    }
}
