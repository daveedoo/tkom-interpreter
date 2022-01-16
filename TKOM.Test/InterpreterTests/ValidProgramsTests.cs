using Shouldly;
using System.Collections.Generic;
using TKOM.Node;
using Xunit;
using Type = TKOM.Node.Type;

namespace TKOMTest.InterpreterTests
{
    public class ValidProgramsTests : InterpreterTestsBase
    {
        [Fact]
        public void PrintIntValue()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new FunctionCall("print", new List<IExpression> { new IntConst(10) })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("10");
        }
        [Fact]
        public void PrintStringValue()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new FunctionCall("print", new List<IExpression> { new StringConst("abcd") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("abcd");
        }
        [Fact]
        public void AssignmentOfConstValue()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a", new IntConst(7)),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("7");
        }
        [Fact]
        public void AssignmentOfVariable()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Declaration(Type.Int, "b"),
                new Assignment("b", new IntConst(123)),
                new Assignment("a", new Variable("b")),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("123");
        }
        [Fact]
        public void LogicalOr_1_WhenLeftIsDifferentThanZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(new IntConst(1), new IntConst(0))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void LogicalOr_1_WhenRightIsDifferentThanZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(new IntConst(0), new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void LogicalOr_0_WhenLeftAndRightEqualZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(new IntConst(0), new IntConst(0))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("0");
        }

        [Fact]
        public void LogicalAnd_1_WhenLeftAndRightIsDifferentThanZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalAnd(new IntConst(1), new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void LogicalAnd_0_WhenLeftEqualZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalAnd(new IntConst(0), new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("0");
        }
        [Fact]
        public void LogicalAnd_0_WhenRightEqualZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalAnd(new IntConst(1), new IntConst(0))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("0");
        }

        [Fact]
        public void EqualityOperator_Equality()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new EqualityOperator(new IntConst(1), EqualityOperatorType.Equality, new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void EqualityOperator_Inequality()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new EqualityOperator(new IntConst(1), EqualityOperatorType.Inequality, new IntConst(2))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }

        [Fact]
        public void RelationOperator_LessEqual()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(new IntConst(1), RelationOperatorType.LessEqual, new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void RelationOperator_GreaterEqual()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(new IntConst(1), RelationOperatorType.GreaterEqual, new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void RelationOperator_LessThan()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(new IntConst(1), RelationOperatorType.LessThan, new IntConst(2))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
        [Fact]
        public void RelationOperator_GreaterThan()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new RelationOperator(new IntConst(1), RelationOperatorType.GreaterThan, new IntConst(2))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }

        [Fact]
        public void AdditiveOperator_Add()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(new IntConst(1), AdditiveOperatorType.Add, new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("2");
        }
        [Fact]
        public void AdditiveOperator_Subtract()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new AdditiveOperator(new IntConst(1), AdditiveOperatorType.Subtract, new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("0");
        }

        [Fact]
        public void MultiplicativeOperator_Multiply()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(new IntConst(5), MultiplicativeOperatorType.Multiply, new IntConst(7))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("35");
        }
        [Fact]
        public void MultiplicativeOperator_Divide()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new MultiplicativeOperator(new IntConst(40), MultiplicativeOperatorType.Divide, new IntConst(8))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("5");
        }

        [Fact]
        public void UnaryOperator_Uminus()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new UnaryOperator(UnaryOperatorType.Uminus, new IntConst(5))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("-5");
        }
        [Fact]
        public void UnaryOperator_LogicalNot()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new UnaryOperator(UnaryOperatorType.LogicalNegation, new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("0");
        }

        [Fact]
        public void IfStatement_ConditionTrue()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new EqualityOperator(new IntConst(1), EqualityOperatorType.Equality, new IntConst(1)),
                    new FunctionCall("print", new List<IExpression> { new StringConst("success") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("success");
        }
        [Fact]
        public void IfStatement_ConditionFalse()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new EqualityOperator(new IntConst(1), EqualityOperatorType.Inequality, new IntConst(1)),
                    new FunctionCall("print", new List<IExpression> { new StringConst("success") }),
                    new FunctionCall("print", new List<IExpression> { new StringConst("failure") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("failure");
        }
        [Fact]
        public void IfStatement_Return()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new IntConst(1), new Block(new List<IStatement> {                       // if(1) {
                        new ReturnStatement(),                                                          //      return;
                        new FunctionCall("print", new List<IExpression> { new StringConst("a") })       //      print("a");
                }))                                                                                     // }
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void WhileStatement_ConditionFalse_DoesNothing()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new IntConst(0),
                    new FunctionCall("print", new List<IExpression> { new StringConst("true") }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void WhileStatement_ConditionTrue()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),                                                                                     // int a;
                new Assignment("a", new IntConst(2)),                                                                               // a = 2;
                new WhileStatement(new Variable("a"), new Block(new List<IStatement>                                                // while (a)
                {                                                                                                                   // {
                    new FunctionCall("print", new List<IExpression> { new Variable("a") }),                                         //  print(a);
                    new Assignment("a", new AdditiveOperator(new Variable("a"), AdditiveOperatorType.Subtract, new IntConst(1)))    //  a = a - 1;
                }))                                                                                                                 // }
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("21");
        }
        [Fact]
        public void WhileStatement_Return()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new IntConst(1), new Block(new List<IStatement>                                                  // while (1)
                {                                                                                                                   // {
                    new ReturnStatement(),                                                                                          //  return;
                    new FunctionCall("print", new List<IExpression> { new StringConst("a") })                                       //  print("a");
                }))                                                                                                                 // }
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void FunctionCall_ReturnStatement_ReturnFromOneFunctionOnly()
        {
            var foo = new FunctionDefinition(Type.Void, "foo", new List<Parameter>(), new Block(new List<IStatement>
            {
                new ReturnStatement()
            }));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new FunctionCall("foo", new List<IExpression>()),
                new FunctionCall("print", new List<IExpression> { new StringConst("a") })
            }));
            Program program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("a");
        }

        [Fact]
        public void ThrowStatement_InsideIfStatement()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new IfStatement(new IntConst(1), new Block(new List<IStatement>                                                     // if (1)
                {                                                                                                                   // {
                    new ThrowStatement(new IntConst(1)),                                                                            //  throw 1;
                    new FunctionCall("print", new List<IExpression> { new StringConst("a") })                                       //  print("a");
                }))                                                                                                                 // }
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void ThrowStatement_InsideWhileStatement()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new WhileStatement(new IntConst(1), new Block(new List<IStatement>                                                  // while (1)
                {                                                                                                                   // {
                    new ThrowStatement(new IntConst(1)),                                                                            //  throw 1;
                    new FunctionCall("print", new List<IExpression> { new StringConst("a") })                                       //  print("a");
                }))                                                                                                                 // }
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void ThrowStatement_FromNotVoidFunctionCall()
        {
            var foo = new FunctionDefinition(Type.Int, "foo", new List<Parameter>(), new Block(new List<IStatement>
            {
                new ThrowStatement(new IntConst(1)),
                new ReturnStatement(new IntConst(2))
            }));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new FunctionCall("foo", new List<IExpression>()),
                new FunctionCall("print", new List<IExpression> { new StringConst("a") })
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }
        [Fact]
        public void ThrowStatement_FromFunctionCallArgument()
        {
            var foo = new FunctionDefinition(Type.Int, "foo", new List<Parameter>(), new Block(new List<IStatement>
            {
                new ThrowStatement(new IntConst(1)),
                new ReturnStatement(new IntConst(2))
            }));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new FunctionCall("print", new List<IExpression> { new FunctionCall("foo", new List<IExpression>()) })
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe(string.Empty);
        }

        [Fact]
        public void TCFStatement()
        {

        }
    }
}
