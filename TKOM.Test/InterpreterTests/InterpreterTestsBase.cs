using System.Collections.Generic;
using TKOM.Interpreter;
using TKOM.Node;
using Type = TKOM.Node.Type;

namespace TKOMTest.InterpreterTests
{
    public abstract class InterpreterTestsBase
    {
        protected readonly ErrorCollector errorHandler;
        protected readonly OutputCollector outputCollector;
        protected readonly Interpreter sut;

        public InterpreterTestsBase()
        {
            errorHandler = new ErrorCollector();
            outputCollector = new OutputCollector();
            sut = new Interpreter(errorHandler, outputCollector);
        }

        public static Program BuildMain(List<IStatement> statements)
        {
            return new Program(new List<FunctionDefinition>
            {
                new FunctionDefinition(
                    Type.Void,
                    "main",
                    new List<Parameter>(),
                    new Block(statements))
            });
        }
    }
}
