﻿using Shouldly;
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
    }
}
