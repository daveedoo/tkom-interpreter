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
            FunctionDefinition funDefVoid = new FunctionDefinition(null, "foo", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>()));
            FunctionDefinition funDefInt = new FunctionDefinition(Type.Int, "foo", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>
                {
                    new ReturnStatement(new IntConst(0))
                }));
            FunctionDefinition main = new FunctionDefinition(null, "main", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>()));
            Program program = new Program(new List<FunctionDefinition>
            {
                funDefVoid, funDefInt, main
            });

            sut.Visit(program);

            errorHandler.errorCount.ShouldBe(1);
        }
        [Fact]
        public void WhenProgramHasMultipleEntryPoints_ThrowsAnError()
        {
            FunctionDefinition main1 = new FunctionDefinition(null, "main", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>
                {
                    new ReturnStatement()
                }));
            FunctionDefinition main2 = new FunctionDefinition(Type.Int, "main", new List<TKOM.Node.Parameter> { },
                new Block(new List<IStatement>
                {
                    new ReturnStatement(new IntConst(0))
                }));
            Program program = new Program(new List<FunctionDefinition>
            {
                main1, main2
            });

            sut.Visit(program);

            errorHandler.errorCount.ShouldBe(1);
        }
        [Fact]
        public void FunctionWithNotnullReturnType_WithoutReturnStatement()
        {
            Program program = new Program(new List<FunctionDefinition>
            {
                new FunctionDefinition(Type.Int, "main", new List<TKOM.Node.Parameter>(), new Block(new List<IStatement>
                {

                }))
            });

            sut.Visit(program);

            errorHandler.errorCount.ShouldBe(1);
        }
    }
}
