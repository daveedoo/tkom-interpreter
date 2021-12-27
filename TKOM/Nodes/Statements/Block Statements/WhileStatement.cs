namespace TKOM.Node
{
    public class WhileStatement : IStatement
    {
        public IExpression condition { get; }
        public IStatement Statement { get; }

        public WhileStatement(IExpression condition, IStatement statement)
        {
            this.condition = condition;
            Statement = statement;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
