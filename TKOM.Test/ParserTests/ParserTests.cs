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


        public static TheoryData<string> invalidPrograms => new TheoryData<string>
        {
            "",                         // empty program
            "int return() {}"           // keyword as identifier
        };

        [Fact]
        public void EmptyFunction()
        {
            string program = "int main() {}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>()))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void FunctionWithSingleParam()
        {
            string program = "int main(int a) {}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>
                    {
                        new Parameter(Type.IntType, "a")
                    }, new Block(new List<IStatement>()))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void FunctionWithMultipleParameters()
        {
            string program = "int main(int a, int b) {}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>
                    {
                        new Parameter(Type.IntType, "a"),
                        new Parameter(Type.IntType, "b")
                    }, new Block(new List<IStatement>()))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Declaration()
        {
            string program = "int main()" +
                "{" +
                "   int a;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Declaration(Type.IntType, "a")
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void MultipleInstructionsInBlock()
        {
            string program = "int main()" +
                "{" +
                "   int a;" +
                "   int b;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Declaration(Type.IntType, "a"),
                        new Declaration(Type.IntType, "b")
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Assignment()
        {
            string program = "int main()" +
                "{" +
                "   int a;" +
                "   a = 7;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Declaration(Type.IntType, "a"),
                        new Assignment("a", new IntConst(7))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void ReturnStatement()
        {
            string program = "int main()" +
                "{" +
                "   return 10;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Return(new IntConst(10))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void BasicFunctionCall()
        {
            string program = "int main()" +
                "{" +
                "   foo();" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new FunctionCall("foo", new List<IExpression>())
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void FunctionCallWithParameter()
        {
            string program = "int main()" +
                "{" +
                "   foo(a);" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new FunctionCall("foo", new List<IExpression>{ new Variable("a") })
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void FunctionCallWithMultipleParameters()
        {
            string program = "int main()" +
                "{" +
                "   foo(a, b);" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new FunctionCall("foo", new List<IExpression>{ new Variable("a"), new Variable("b") })
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
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
