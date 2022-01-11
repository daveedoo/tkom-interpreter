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
            var foo = new FunctionDefinition(Type.Void, "foo", new List<Parameter>(), new Block(new List<IStatement>()));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(
                        new FunctionCall("foo", new List<IExpression>()),
                        new IntConst(1)))
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
        [Fact]
        public void LogicalOr_RightIsNotIntType()
        {
            var foo = new FunctionDefinition(Type.Void, "foo", new List<Parameter>(), new Block(new List<IStatement>()));
            var main = new FunctionDefinition(Type.Void, "main", new List<Parameter>(), new Block(new List<IStatement>
            {
                new Declaration(Type.Int, "a"),
                new Assignment("a",
                    new LogicalOr(
                        new IntConst(1),
                        new FunctionCall("foo", new List<IExpression>())))
            }));
            var program = new Program(new List<FunctionDefinition>
            {
                foo, main
            });

            program.Accept(sut);

            errorHandler.errorsCount.ShouldBe(1);
        }
    }
}
