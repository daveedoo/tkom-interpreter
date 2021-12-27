namespace TKOM.Node
{
    public interface INodeVisitor
    {
        public void Visit(INode node);
    }

    public interface INode
    {
        public void Accept(INodeVisitor visitor);
    }
}
