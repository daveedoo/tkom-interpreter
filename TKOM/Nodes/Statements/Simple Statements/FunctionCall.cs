using System.Collections.Generic;

namespace TKOM.Node
{
    public class FunctionCall : IStatement, IExpression
    {
        public string Identifier { get; set; }
        public IList<IExpression> Arguments { get; }

        public FunctionCall(string identifier, IList<IExpression> arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
