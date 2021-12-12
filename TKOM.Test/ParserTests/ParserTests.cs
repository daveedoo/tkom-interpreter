using Shouldly;
using System;
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

        public static TheoryData<string, Program> validPrograms => new TheoryData<string, Program>
        {
            { "int main() {}",
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(TKOM.Node.Type.IntType, "main", new List<Parameter>())
                })},
            { "int main(int a) {}",
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(TKOM.Node.Type.IntType, "main", new List<Parameter>
                    {
                        new Parameter(TKOM.Node.Type.IntType, "a")
                    })
                }) }
        };

        public static TheoryData<string> invalidPrograms => new TheoryData<string>
        {
            "",                         // empty program
            "int return() {}"           // keyword as identifier
        };

        private IParser buildParser(string program)
        {
            errorHandler = new ErrorCollecter();
            TextReader reader = new StringReader(program);
            IScanner scanner = new Scanner(reader, errorHandler);
            return new Parser(scanner, errorHandler);
        }

        [Theory]
        [MemberData(nameof(validPrograms))]
        public void ValidProgram_ShouldCreateProperAST_AndReturnTrue(string program, Program expected)
        {
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(validPrograms))]
        public void ValidProgram_ShouldntThrowAnyErrors(string program, Program expected)
        {
            IParser parser = buildParser(program);

            parser.TryParse(out Program actualTree);

            errorHandler.errorCount.ShouldBe(0);
        }


        [Theory]
        [MemberData(nameof(invalidPrograms))]
        public void InvalidProgram_ShouldSetASTToNull(string program)
        {
            IParser parser = buildParser(program);

            parser.TryParse(out Program ast);

            ast.ShouldBeNull();
        }
        [Theory]
        [MemberData(nameof(invalidPrograms))]
        public void InvalidProgram_ShouldReturnFalse(string program)
        {
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program _);

            parsed.ShouldBeFalse();
        }
        [Theory]
        [MemberData(nameof(invalidPrograms))]
        public void InvalidProgram_ShouldThrowError(string program)
        {
            IParser parser = buildParser(program);

            parser.TryParse(out Program _);

            errorHandler.errorCount.ShouldBeGreaterThan(0);
        }
    }
}
