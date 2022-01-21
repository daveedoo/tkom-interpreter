using System;
using TKOM.Node;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    public class ReadFunction : Function
    {
        public ReadFunction(Type valueToReadType, string paramName) : base(Type.Void, "read", new Parameter(valueToReadType, paramName))
        { }
        public override void Accept(Interpreter visitor)
        {
            visitor.Visit(this);
        }
    }
}
