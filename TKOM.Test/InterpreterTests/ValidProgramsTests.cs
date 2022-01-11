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
            Program program = new Program(new List<FunctionDefinition>
            {
                new FunctionDefinition(Type.Void, "main", new List<Parameter>(),
                    new Block(new List<IStatement>
                    {
                        new FunctionCall("print", new List<IExpression> { new IntConst(10) })
                    }))
            });

            program.Accept(sut);
            string output = outputCollector.GetOutput();

            errorHandler.errorCount.ShouldBe(0);
            output.ShouldBe("10");
        }
    }
}
