namespace TKOM.Node
{
    public class LogicalOr : BinaryOperator
    {
        public LogicalOr(IExpression left, IExpression right) : base(left, right)
        { }
        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
