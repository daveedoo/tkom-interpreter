using Shouldly;
using System.Collections.Generic;
using TKOM.Node;
using TKOM.Parser;
using Xunit;

namespace TKOMTest.ParserTests
{
    public class ValidPrograms : ParserTests
    {
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
        public void ConsecutiveTryParseCall_ReturnsFalse()
        {
            IParser parser = buildParser("");

            parser.TryParse(out Program _);
            bool parsed2 = parser.TryParse(out Program program);

            parsed2.ShouldBeFalse();
            program.ShouldBeNull();
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
                "   a = 7;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
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
                        new ReturnStatement(new IntConst(10))
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
                        new ReturnStatement()
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
                        new ThrowStatement(new IntConst(10))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void IfStatementBasic()
        {
            string program = "int main()" +
                "{" +
                "   if (1)" +
                "       a = 0;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new IfStatement(new IntConst(1), new Assignment("a", new IntConst(0)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void IfStatementWithBlockStatement()
        {
            string program = "int main()" +
                "{" +
                "   if (1)" +
                "   {" +
                "       a = 0;" +
                "   }" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new IfStatement(new IntConst(1), new Block(new List<IStatement>{
                            new Assignment("a", new IntConst(0))
                        }))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void IfStatementWithElse()
        {
            string program = "int main()" +
                "{" +
                "   if (1)" +
                "       a = 1;" +
                "   else" +
                "       a = 0;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new IfStatement(new IntConst(1),
                            new Assignment("a", new IntConst(1)),
                            new Assignment("a", new IntConst(0))
                        )
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void WhileStatement()
        {
            string program = "int main()" +
                "{" +
                "   while(1)" +
                "       return;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new WhileStatement(new IntConst(1), new ReturnStatement())
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void TryCatchStatement()
        {
            string program = "int main()" +
                "{" +
                "   try {" +
                "       a = 10;" +
                "   } catch Exception e" +
                "       int x;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new TryCatchFinally(new Block(new List<IStatement>{
                            new Assignment("a", new IntConst(10)) }), 
                        new List<Catch>
                            {
                                new Catch("e", new Declaration(Type.IntType, "x"))
                            })
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void TryCatchFinallyMoreComplex()
        {
            string program = "int main()" +
                "{" +
                "   try {" +
                "       a = 10;" +
                "   } catch Exception e when 1" +
                "       int x;" +
                "   catch Exception f {" +
                "   }" +
                "   finally {" +
                "       int k;" +
                "       foo(k);" +
                "   }" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new TryCatchFinally(new Block(new List<IStatement>{
                            new Assignment("a", new IntConst(10)) }),
                        new List<Catch>
                        {
                            new Catch("e", new Declaration(Type.IntType, "x"), new IntConst(1)),
                            new Catch("f", new Block(new List<IStatement>()))
                        },
                        new Block(new List<IStatement>
                        {
                            new Declaration(Type.IntType, "k"),
                            new FunctionCall("foo", new List<IExpression>{ new Variable("k") })
                        }))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }

        // OPERATORS
        [Fact]
        public void LogicalOr()
        {
            string program = "int main()" +
                "{" +
                "   a = 1 || 2;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a", new LogicalOr(new IntConst(1), new IntConst(2)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void LogicalOrWithIdentifiers()
        {
            string program = "int main()" +
                "{" +
                "   a = b || c;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a", new LogicalOr(new Variable("b"), new Variable("c")))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void MultipleLogicalOr()
        {
            string program = "int main()" +
                "{" +
                "   a = 1 || 2 || 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new LogicalOr(
                                new LogicalOr(new IntConst(1), new IntConst(2)),
                                new IntConst(3))
                            )
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void MultipleLogicalOrWithIdentifiers()
        {
            string program = "int main()" +
                "{" +
                "   a = b || c || d;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new LogicalOr(
                                new LogicalOr(new Variable("b"), new Variable("c")),
                                new Variable("d"))
                            )
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void LogicalAnd()
        {
            string program = "int main()" +
                "{" +
                "   a = 1 && 2;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a", new LogicalAnd(new IntConst(1), new IntConst(2)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void MultipleLogicalAnd()
        {
            string program = "int main()" +
                "{" +
                "   a = 1 && 2 && 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new LogicalAnd(
                                new LogicalAnd(new IntConst(1), new IntConst(2)),
                                new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void EqualityOperator()
        {
            string program = "int main()" +
                "{" +
                "   a = 1 == 2;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a", new EqualityOperator(new IntConst(1), EqualityOperatorType.Equality, new IntConst(2)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void InequalityOperator()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 != 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new EqualityOperator(new IntConst(2), EqualityOperatorType.Inequality, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void RelationLessEqual()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 <= 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new RelationOperator(
                                new IntConst(2), RelationOperatorType.LessEqual, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void RelationGreaterEqual()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 >= 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new RelationOperator(
                                new IntConst(2), RelationOperatorType.GreaterEqual, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void RelationLessThan()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 < 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new RelationOperator(
                                new IntConst(2), RelationOperatorType.LessThan, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void RelationGreaterThan()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 > 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new RelationOperator(
                                new IntConst(2), RelationOperatorType.GreaterThan, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Add()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 + 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new AdditiveOperator(
                                new IntConst(2), AdditiveOperatorType.Add, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Subtract()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 - 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new AdditiveOperator(
                                new IntConst(2), AdditiveOperatorType.Subtract, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Multiply()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 * 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new MultiplicativeOperator(
                                new IntConst(2), MultiplicativeOperatorType.Multiply, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Divide()
        {
            string program = "int main()" +
                "{" +
                "   a = 2 / 3;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new MultiplicativeOperator(
                                new IntConst(2), MultiplicativeOperatorType.Divide, new IntConst(3)))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Unary_Minus()
        {
            string program = "int main()" +
                "{" +
                "   a = -b;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new UnaryOperator(
                                UnaryOperatorType.Uminus, new Variable("b")))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Unary_Negation()
        {
            string program = "int main()" +
                "{" +
                "   a = !b;" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                            new UnaryOperator(
                                UnaryOperatorType.LogicalNegation, new Variable("b")))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Brackets()
        {
            string program = "int main()" +
                "{" +
                "   a = (b);" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                                new Variable("b"))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
        [Fact]
        public void Atomic_FunctionCall()
        {
            string program = "int main()" +
                "{" +
                "   a = foo();" +
                "}";
            Program ast = new Program(new List<FunctionDefinition>
                {
                    new FunctionDefinition(Type.IntType, "main", new List<Parameter>(), new Block(new List<IStatement>
                    {
                        new Assignment("a",
                                new FunctionCall("foo", new List<IExpression>()))
                    }))
                });
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program actualTree);

            parsed.ShouldBeTrue();
            actualTree.ShouldBeEquivalentTo(ast);
            errorHandler.errorCount.ShouldBe(0);
        }
    }
}
