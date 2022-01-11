using System.Collections.Generic;
using TKOM.Node;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    public class PrintFunction : Function
    {
        public const string paramName = "printVal";

        public PrintFunction(Type valueToPrintType) : base( Type.Void, "print", new List<Parameter>
            {
                new Parameter(valueToPrintType, paramName)
            })
        { }

        public override void Accept(Interpreter visitor)
        {
            visitor.Visit(this);
        }
    }
}
