using System.Collections.Generic;
using System.IO;
using TKOM.Interpreter;
using TKOM.Node;
using TKOMTest.Utils;
using Type = TKOM.Node.Type;

namespace TKOMTest.InterpreterTests
{
    public abstract class InterpreterTestsBase
    {
        protected readonly ErrorsCollector errorsCollector = new();
        protected readonly OutputCollector outputCollector = new();
        private Interpreter _sut;
        protected Interpreter sut { get => _sut; }

        protected void SetInterpreter(string input = "")
        {
            _sut = new Interpreter(errorsCollector, outputCollector, new StringReader(input));
        }

        public static Program BuildMainOnlyProgram(List<IStatement> statements)
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
