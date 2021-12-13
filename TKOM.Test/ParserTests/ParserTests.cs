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

        public class TestCase
        {
            public string Program { get; }
            public Program AST { get; }

            public TestCase(string program, Program ast)
            {
                Program = program;
                AST = ast;
            }
        }

        public static TheoryData<TestCase> validPrograms => new TheoryData<TestCase>
        {
            new TestCase("int main() {}",                              // empty function
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>()))
                })),
            new TestCase("int main(int a) {}",                         // function with single param
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>
                    {
                        new Parameter(Type.IntType, "a")
                    }, new Block(new List<IStatement>()))
                })),
            new TestCase("int main(int a, int b) {}",                  // function with more params
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>
                    {
                        new Parameter(Type.IntType, "a"),
                        new Parameter(Type.IntType, "b")
                    }, new Block(new List<IStatement>()))
                })),
            new TestCase("int main()" +                                 // declaration
                "{" +
                "   int a;" +
                "}",
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Declaration(Type.IntType, "a")
                    }))
                })),
            new TestCase("int main()" +                                 // multiple instructions in block
                "{" +
                "   int a;" +
                "   int b;" +
                "}",
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Declaration(Type.IntType, "a"),
                        new Declaration(Type.IntType, "b")
                    }))
                })),
            new TestCase("int main()" +                                 // assignment
                "{" +
                "   int a;" +
                "   a = 7;" +
                "}",
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Declaration(Type.IntType, "a"),
                        new Assignment("a", new IntConst(7))
                    }))
                })),
            new TestCase("int main()" +                                 // return
                "{" +
                "   return 10;" +
                "}",
                new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Return(new IntConst(10))
                    }))
                }))
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
        public void ValidProgram_ShouldCreateProperAST_ReturnTrue_NoErrors(TestCase testCase)
        {
            IParser parser = buildParser(testCase.Program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(testCase.AST);
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
