using Shouldly;
using System.Collections.Generic;
using TKOM.Node;
using Xunit;
using Type = TKOM.Node.Type;

namespace TKOMTest.InterpreterTests
{
    public class InvalidProgramsTests : InterpreterTestsBase
    {
        public InvalidProgramsTests() : base()
        { }

        [Fact]
        public void WhenProgramHasAmbigousFunctionDefinitions_ThrowsAnError()
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

            errorHandler.errorCount.ShouldBe(1);
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

            errorHandler.errorCount.ShouldBe(1);
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

            errorHandler.errorCount.ShouldBe(1);
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

            errorHandler.errorCount.ShouldBe(1);
        }
        [Fact]
        public void WhenCallingErroneousFunction_ThrowsOneErrorOnly()
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

            errorHandler.errorCount.ShouldBe(1);
        }
    }
}
