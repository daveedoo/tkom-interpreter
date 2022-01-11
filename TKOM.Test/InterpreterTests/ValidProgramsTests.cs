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
        public void LogicalOr_1_WhenLeftIsGreaterThanZero()
        {
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(new IntConst(1), new IntConst(1))),
                new FunctionCall("print", new List<IExpression>{ new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("1");
        }
    }
}
