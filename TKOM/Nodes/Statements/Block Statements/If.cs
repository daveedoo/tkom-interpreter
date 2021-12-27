namespace TKOM.Node
{
    public class If : IStatement
    {
        public IExpression Condition { get; }
        public IStatement IfStatement { get; }
        public IStatement ElseStatement { get; }

        public If(IExpression condition, IStatement ifStatement, IStatement elseStatement = null)
        {
            Condition = condition;
            IfStatement = ifStatement;
            ElseStatement = elseStatement;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
