namespace TKOM.Node
{
    public class ThrowStatement : IStatement
    {
        IExpression Expression { get; }

        public ThrowStatement(IExpression expression)
        {
            Expression = expression;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
