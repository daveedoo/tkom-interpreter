namespace TKOM.Node
{
    public class LogicalAnd : BinaryOperator
    {
        public LogicalAnd(IExpression left, IExpression right) : base(left, right)
        { }
        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
