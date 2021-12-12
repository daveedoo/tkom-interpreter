using Shouldly;
using System.Collections.Generic;
using System.IO;
using TKOM.Node;
using TKOM.Scanner;
using Xunit;

namespace TKOM.Parser.Test
{
    public class ParserTests
    {
        private ErrorHandler.ErrorHandler errorHandler;

        private IParser buildParser(string program)
        {
            errorHandler = new ErrorHandler.ErrorHandler();
            TextReader reader = new StringReader(program);
            IScanner scanner = new Scanner.Scanner(reader, errorHandler);
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
            Program expectedProgram = new Program(new List<FunctionDefinition>());

            parser.TryParse(out Program program);

            program.ShouldBeEquivalentTo(expectedProgram);
        }
    }
}
