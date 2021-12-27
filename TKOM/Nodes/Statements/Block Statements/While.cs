namespace TKOM.Node
{
    public class While : IStatement
    {
        public IExpression condition { get; }
        public IStatement Statement { get; }

        public While(IExpression condition, IStatement statement)
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
