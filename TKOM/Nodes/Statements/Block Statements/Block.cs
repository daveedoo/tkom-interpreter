using System.Collections.Generic;

namespace TKOM.Node
{
    public class Block : IStatement
    {
        public IList<IStatement> Statements { get; }

        public Block(IList<IStatement> statements)
        {
            Statements = statements;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
