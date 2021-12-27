namespace TKOM.Node
{
    public class Variable : IExpression
    {
        public string Identifier { get; }

        public Variable(string identifier)
        {
            Identifier = identifier;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
