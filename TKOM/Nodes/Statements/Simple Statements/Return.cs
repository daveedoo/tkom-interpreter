namespace TKOM.Node
{
    public class Return : IStatement
    {
        public IExpression Expression { get; }

        public Return(IExpression expression = null)
        {
            Expression = expression;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
