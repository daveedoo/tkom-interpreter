using Shouldly;
using System.Collections.Generic;
using TKOM.Node;
using Xunit;

namespace TKOMTest.InterpreterTests
{
    public class InputTests : InterpreterTestsBase
    {
        [Fact]
        public void ReadIntValue()
        {
            SetInterpreter("10");
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new FunctionCall("read", new List<IExpression>{ new Variable("a") }),
                new FunctionCall("print", new List<IExpression>() { new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("10");
        }
        [Fact]
        public void ReadIntValue_IgnoresWhiteCharacters()
        {
            SetInterpreter(" 10");
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new FunctionCall("read", new List<IExpression>{ new Variable("a") }),
                new FunctionCall("print", new List<IExpression>() { new Variable("a") })
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorsCount.ShouldBe(0);
            output.ShouldBe("10");
        }
        [Fact]
        public void ReadIntValue_WhenNotDigitCharacter()
        {
            SetInterpreter("a");
            var program = BuildMainOnlyProgram(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new FunctionCall("read", new List<IExpression>{ new Variable("a") }),
                new FunctionCall("print", new List<IExpression>() { new Variable("a") })
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
    }
}
