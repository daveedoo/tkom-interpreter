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
            "",                                 // empty program
            "int main",                         // incomplete function
            "int return() {}",                  // keyword as identifier
            "int main() {} &&",                 // illegal token at the end
            "int main ( )",                     // no block
            "int main(int) {}",                 // incomplete parameter
            "int main(int a {}",                // not closed parameters list
            "int main(int a,) {}",              // parameter with additional comma
            "int main(void a) {}",              // void type parameter
            "int main() { int; }",              // incomplete declaration
            "int main() { a =; }",              // incomplete assignment
            "int main() { a =5 }",              // instruction without semicolon
            "int main() { foo(; }",             // incomplete function call
            "int main() { foo; }",              // incomplete function call
            "int main() { foo(2 int); }",       // function call with incorrect argument
            "int main() { throw; }",            // incomplete throw
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
        public void EmptyVoidFunction()
        {
            string program = "void main() {}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>()))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void MultipleFunctions()
        {
            string program = "int foo() {}" +
                             "int main() {}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "foo", new List<Parameter>(), new Block(new List<IStatement>())),
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
        public void ReturnVoid()
        {
            string program = "int main()" +
                "{" +
                "   return;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Return()
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
        [Fact]
        public void ThrowStatement()
        {
            string program = "int main()" +
                "{" +
                "   throw Exception(10);" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Throw(new IntConst(10))
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
