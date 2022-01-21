using System.IO;
using TKOM.Parser;
using TKOM.Scanner;

namespace TKOMTest.ParserTests
{
    public abstract class ParserTestsBase
    {
        protected ErrorsCollector errorsCollector;

        protected IParser buildParser(string program)
        {
            errorsCollector = new ErrorsCollector();
            TextReader reader = new StringReader(program);
            IScanner scanner = new Scanner(reader, errorsCollector);
            return new Parser(scanner, errorsCollector);
        }
    }
}
