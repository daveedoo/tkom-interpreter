namespace TKOM.Node
{
    public class IfStatement : IStatement
    {
        public IExpression Condition { get; }
        public IStatement TrueStatement { get; }
        public IStatement ElseStatement { get; }

        public IfStatement(IExpression condition, IStatement trueStatement, IStatement elseStatement = null)
        {
            Condition = condition;
            TrueStatement = trueStatement;
            ElseStatement = elseStatement;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
