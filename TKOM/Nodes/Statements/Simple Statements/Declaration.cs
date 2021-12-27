namespace TKOM.Node
{
    public class Declaration : IStatement
    {
        public Type Type { get; }
        public string Name { get; }
        public Declaration(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
