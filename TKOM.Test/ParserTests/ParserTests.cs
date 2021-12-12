using Shouldly;
using System.Collections.Generic;
using System.IO;
using TKOM.Node;
using TKOM.Parser;
using TKOM.Scanner;
using Xunit;

namespace TKOMTest.ParserTests
{
    public class ParserTests
    {
        private ErrorCollecter errorHandler;

        private IParser buildParser(string program)
        {
            errorHandler = new ErrorCollecter();
            TextReader reader = new StringReader(program);
            IScanner scanner = new Scanner(reader, errorHandler);
            return new Parser(scanner, errorHandler);
        }

        [Fact]
        public void WhenEmptyProgram_ReturnsFalse()
        {
            IParser parser = buildParser("");

            bool parsedSuccessfully = parser.TryParse(out _);

            parsedSuccessfully.ShouldBe(false);
        }
        [Fact]
        public void WhenEmptyProgram_SetsOutParamToNull()
        {
            IParser parser = buildParser("");

            parser.TryParse(out Program program);

            program.ShouldBeNull();
        }

        [Fact]
        public void TryParseFunctionDefinition_WhenEmptyFunctionDefinition_ShouldHaveSingleItem()
        {
            IParser parser = buildParser("int main() {}");
            List<FunctionDefinition> functions = new List<FunctionDefinition>
            {
                new FunctionDefinition(Type.IntType, "main", new List<Parameter>())
            };
            Program expectedProgram = new Program(functions);

            parser.TryParse(out Program program);

            program.ShouldBeEquivalentTo(expectedProgram);
        }

        [Fact]
        public void TryParseFunctionDefinition_WhenKeywordAsIdentifier_ShouldReturnFalse()
        {
            IParser parser = buildParser("int return() {}");

            bool parsed = parser.TryParse(out Program program);

            parsed.ShouldBeFalse();
        }

        [Fact]
        public void TryParseFunctionDefinition_WhenKeywordAsIdentifier_ShouldThrowError()
        {
            IParser parser = buildParser("int return() {}");

            parser.TryParse(out Program program);
            
            errorco
        }
    }
}
