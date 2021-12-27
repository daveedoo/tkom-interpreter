namespace TKOM.Node
{
    public class ReturnStatement : IStatement
    {
        public IExpression Expression { get; }

        public ReturnStatement(IExpression expression = null)
        {
            Expression = expression;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
