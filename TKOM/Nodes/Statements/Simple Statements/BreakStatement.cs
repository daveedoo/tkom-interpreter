namespace TKOM.Node
{
    public class BreakStatement : INode, IStatement
    {
        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
