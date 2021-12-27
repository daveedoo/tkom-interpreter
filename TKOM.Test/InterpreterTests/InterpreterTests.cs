using Shouldly;
using System.Collections.Generic;
using TKOM.Interpreter;
using TKOM.Node;
using Xunit;
using Type = TKOM.Node.Type;

namespace TKOMTest.InterpreterTests
{
    public class InterpreterTests
    {
        private readonly ErrorCollecter errorHandler;
        private readonly Interpreter sut;

        public InterpreterTests()
        {
            errorHandler = new ErrorCollecter();
            sut = new Interpreter(errorHandler);
        }

        [Fact]
        public void WhenProgramHasAmbigousFunctionDefinitions_ThrowsAnError()
        {
            FunctionDefinition funDefVoid = new FunctionDefinition(Type.Void, "foo", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>()));
            FunctionDefinition funDefInt = new FunctionDefinition(Type.IntType, "foo", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>()));
            FunctionDefinition main = new FunctionDefinition(Type.Void, "main", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>()));
            Program program = new Program(new List<FunctionDefinition>
            {
                funDefVoid, funDefInt, main
            });

            sut.Visit(program);

            errorHandler.errorCount.ShouldBe(1);
        }
    }
}
