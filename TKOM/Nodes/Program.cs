using System.Collections.Generic;

namespace TKOM.Node
{
    public class Program : INode
    {
        public IList<FunctionDefinition> functions { get; }

        public Program(IList<FunctionDefinition> functionDefinitions)
        {
            functions = functionDefinitions;
        }
    }
}
