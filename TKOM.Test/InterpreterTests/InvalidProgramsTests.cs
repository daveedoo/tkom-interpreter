using Shouldly;
using System.Collections.Generic;
using TKOM.Node;
using Xunit;
using Type = TKOM.Node.Type;

namespace TKOMTest.InterpreterTests
{
    public class InvalidProgramsTests : InterpreterTestsBase
    {
        public InvalidProgramsTests()
        {
            SetInterpreter();
        }

        [Fact]
        public void AmbigousFunctionDefinitions()
        {
            FunctionDefinition funDefVoid = new(Type.Void, "foo", new List<Parameter> { },
                new Block(new List<IStatement>()));
            FunctionDefinition funDefInt = new(Type.Int, "foo", new List<Parameter> { },
                new Block(new List<IStatement>
                {
                    new ReturnStatement(new IntConst(0))
                }));
            FunctionDefinition main = new(Type.Void, "main", new List<Parameter> { },
                new Block(new List<IStatement>
                {
                    new FunctionCall("foo", new List<IExpression>())
                }));
            Program program = new(new List<FunctionDefinition>
            {
                funDefVoid, funDefInt, main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void ProgramWithoutEntryPoint()
        {
            var program = new Program(new List<FunctionDefinition>
            {
                new FunctionDefinition(Type.Void, "foo", new List<Parameter>(), new Block(new List<IStatement>
                {

                }))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void FunctionWithNotVoidReturnType_WithoutReturnStatement()
        {
            Program program = new(new List<FunctionDefinition>
            {
                new FunctionDefinition(Type.Int, "main", new List<Parameter>(), new Block(new List<IStatement>
                {

                }))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void FunctionWithNotVoidReturnType_WithEmptyReturnStatement()
        {
            Program program = new(new List<FunctionDefinition>
            {
                new FunctionDefinition(Type.Int, "main", new List<Parameter>(), new Block(new List<IStatement>
                {
                    new ReturnStatement()
                }))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void CallingErroneousFunction_ThrowsOneErrorOnly()
        {
            FunctionDefinition foo = new(Type.Int, "foo", new List<Parameter>(),
                new Block(new List<IStatement>
                {
                    new FunctionCall("bar", new List<IExpression>())
                }));
            FunctionDefinition main = new(Type.Void, "main", new List<Parameter>(),
                new Block(new List<IStatement>
                {
                    new FunctionCall("foo", new List<IExpression>())
                }));
            Program program = new(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void FunctionCallWithInvalidParameter_OneErrorOnly()
        {
            var foo = new FunctionDefinition(Type.Void, "foo", new List<Parameter> { new Parameter(Type.Int, "param") }, new Block(new List<IStatement>()));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new FunctionCall("foo", new List<IExpression>{ new Variable("a")})
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void Redeclaration()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Declaration(Type.Int, "a")
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AssignmentToNonexistingVariable()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Assignment("a", new IntConst(5))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AssignmentOfNonexistingVariable()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a", new Variable("b"))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AssignmentOfVoidFunctionCall()
        {
            var emptyFoo = new FunctionDefinition(Type.Void, "foo", new List<Parameter>(), new Block(new List<IStatement>()));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a", new FunctionCall("foo", new List<IExpression>()))
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                emptyFoo,
                main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        #region operators
        [Fact]
        public void LogicalOr_LeftInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(new Variable("b"), new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void LogicalOr_RightInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(new IntConst(1), new Variable("b")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void LogicalOr_LeftIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(
                        new StringConst("xyz"),
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void LogicalOr_RightIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(
                        new IntConst(1),
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void LogicalAnd_LeftInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalAnd(new Variable("b"), new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void LogicalAnd_LeftIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalAnd(
                        new StringConst("xyz"),
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void LogicalAnd_RightIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalAnd(
                        new IntConst(1),
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void EqualityOperator_LeftIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new EqualityOperator(
                        new StringConst("xyz"),
                        EqualityOperatorType.Equality,
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void EqualityOperator_RightIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new EqualityOperator(
                        new IntConst(1),
                        EqualityOperatorType.Equality,
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void EqualityOperator_LeftInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new EqualityOperator(new Variable("b"), EqualityOperatorType.Equality, new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void RelationOperator_LeftIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(
                        new StringConst("xyz"),
                        RelationOperatorType.LessEqual,
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void RelationOperator_RightIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(
                        new IntConst(1),
                        RelationOperatorType.LessEqual,
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void RelationOperator_LeftInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(new Variable("b"), RelationOperatorType.LessEqual, new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void AdditiveOperator_LeftIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(
                        new StringConst("xyz"),
                        AdditiveOperatorType.Add,
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AdditiveOperator_RightIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(
                        new IntConst(1),
                        AdditiveOperatorType.Add,
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AdditiveOperator_LeftInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(new Variable("b"), AdditiveOperatorType.Add, new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AdditiveOperator_Add_CheckedForIntOverflow()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(
                        new IntConst(int.MaxValue),
                        AdditiveOperatorType.Add,
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void AdditiveOperator_Subtract_CheckedForIntOverflow()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(
                        new IntConst(int.MinValue),
                        AdditiveOperatorType.Subtract,
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void MultiplicativeOperator_LeftIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(
                        new StringConst("xyz"),
                        MultiplicativeOperatorType.Multiply,
                        new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void MultiplicativeOperator_RightIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(
                        new IntConst(1),
                        MultiplicativeOperatorType.Multiply,
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void MultiplicativeOperator_LeftInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(new Variable("b"), MultiplicativeOperatorType.Multiply, new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void MultiplicativeOperator_Multiply_CheckedForIntOverflow()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(
                        new IntConst((int.MaxValue / 2) + 1),
                        MultiplicativeOperatorType.Multiply,
                        new IntConst(2)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void MultiplicativeOperator_Divide_CheckedForIntOverflow()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(
                        new IntConst((int.MinValue / 2) - 1),
                        MultiplicativeOperatorType.Multiply,
                        new IntConst(2)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        [Fact]
        public void UnaryOperator_ExpressionIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new UnaryOperator(
                        UnaryOperatorType.Uminus,
                        new StringConst("xyz")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void UnaryOperator_ExpressionInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new UnaryOperator(UnaryOperatorType.Uminus, new Variable("b")))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        #endregion

        #region IfStatement
        [Fact]
        public void IfStatement_ConditionInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new EqualityOperator(new StringConst("1"), EqualityOperatorType.Inequality, new IntConst(1)),
                    new FunctionCall("print", new List<IExpression> { new StringConst("success") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(1);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void IfStatement_ConditionIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new StringConst("1"),
                    new FunctionCall("print", new List<IExpression> { new StringConst("success") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(1);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void IfStatement_EmbeddedStatementIsDeclaration()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new IntConst(1),
                    new Declaration(Type.Int, "a"))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void IfStatement_ElseStatementIsDeclaration()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new IntConst(0),
                    new FunctionCall("print", new List<IExpression> { new StringConst("success") }),
                    new Declaration(Type.Int, "a"))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(1);
        }
        #endregion

        #region WhileStatement
        [Fact]
        public void WhileStatement_ConditionInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new EqualityOperator(new StringConst("1"), EqualityOperatorType.Inequality, new IntConst(1)),
                    new FunctionCall("print", new List<IExpression> { new StringConst("success") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(1);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void WhileStatement_ConditionIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new StringConst("1"),
                    new FunctionCall("print", new List<IExpression> { new StringConst("true") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(1);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void WhileStatement_EmbeddedStatementIsDeclaration()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new IntConst(1),
                    new Declaration(Type.Int, "a"))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void WhileStatement_InvalidEmbeddedStatement_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new IntConst(1),
                    new Assignment("a", new IntConst(1)))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void BreakStatement_OutsideOfWhileStatement()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new BreakStatement()
            });
            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void BreakStatement_InsideOfCalledFunction_AndOutsideOfWhileStatement()
        {
            var foo = new FunctionDefinition(Type.Void, "foo", new List<Parameter>(), new Block(new List<IStatement>
            {
                new BreakStatement()
            }));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new WhileStatement(new IntConst(1),
                    new FunctionCall("foo", new List<IExpression>()))
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        #endregion

        [Fact]
        public void ThrowStatement_ThrowingNotIntValue()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new ThrowStatement(new StringConst("C"))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        #region TryCatchFinally statement
        [Fact]
        public void TryCatchStatement_TryStatementInvalid_OneErrorOnly()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new ThrowStatement(new Variable("a")),
                    new List<Catch>
                    {
                        new Catch("e",
                            new FunctionCall("foo", new List<IExpression>()))
                    })
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void TryCatchStatement_WhenExpressionIsNotIntType()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new ThrowStatement(new IntConst(1)),
                    new List<Catch>
                    {
                        new Catch("e",
                            new FunctionCall("print", new List<IExpression> { new StringConst("A") }), new StringConst("xyz"))
                    })
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void TryCatchStatement_WhenWhenStatementBlockHasError_OneErrorOnly()
        {
            //  try
            //  {
            //      throw 1;
            //  }
            //  catch e when a
            //  {
            //  }
            //  print(b);
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new ThrowStatement(new IntConst(1)),
                    new List<Catch>
                    {
                        new Catch("e",
                            new Block(new List<IStatement>()),
                            new Variable("a"))
                    }),
                    new FunctionCall("print", new List<IExpression> { new StringConst("B") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(1);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void TryCatchStatement_TryEmbeddedStatementIsDeclaration()
        {
            // try
            //     int a;
            // catch e {}
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new Declaration(Type.Int, "a"),
                    new List<Catch>
                    {
                        new Catch("e", new Block(new List<IStatement>()))
                    })
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void TryCatchStatement_CatchWithoutWhen_EmbeddedStatementIsDeclaration()
        {
            // try
            //     throw 1;
            // catch e
            //     int a;
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new ThrowStatement(new IntConst(1)),
                    new List<Catch>
                    {
                        new Catch("e",
                            new Declaration(Type.Int, "a"))
                    })
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void TryCatchStatement_CatchWithWhen_EmbeddedStatementIsDeclaration()
        {
            // try
            //     throw 1;
            // catch e when 1
            //     int a;
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new ThrowStatement(new IntConst(1)),
                    new List<Catch>
                    {
                        new Catch("e",
                            new Declaration(Type.Int, "a"),
                            new IntConst(1))
                    })
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void TryCatchStatement_FinallyEmbeddedStatementIsDeclaration()
        {
            // try
            //     throw 1;
            // catch {}
            // finally
            //     int a;
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new TryCatchFinally(
                    new ThrowStatement(new IntConst(1)),
                    new List<Catch>
                    {
                        new Catch(new Block(new List<IStatement>()),
                            new IntConst(1))
                    },
                    new Declaration(Type.Int, "a"))
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }

        #endregion
    }
}
