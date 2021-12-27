using System;
using System.Collections.Generic;
using TKOM.Node;

namespace TKOM.Interpreter
{
    public class Interpreter : INodeVisitor
    {
        public Stack<FunctionCallContext> CallStack { get; }

        public Interpreter()
        {
            CallStack = new Stack<FunctionCallContext>();

            IntegerValue zero = new IntegerValue(0);
            Variable testVar = new Variable("x", zero);
        }

        public void Visit(INode node)
        {
            throw new ArgumentException("Unsupported node type");
        }

        public void Visit(Program program)
        {

        }
    }
}
