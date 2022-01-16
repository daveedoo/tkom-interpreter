namespace TKOM.Node
{
    public class WhileStatement : IStatement
    {
        public IExpression Condition { get; }
        public IStatement Statement { get; }

        public WhileStatement(IExpression condition, IStatement statement)
        {
            this.Condition = condition;
            Statement = statement;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
