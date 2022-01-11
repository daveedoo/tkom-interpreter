namespace TKOM.Node
{
    public enum EqualityOperatorType
    {
        Equality, Inequality
    }

    public class EqualityOperator : BinaryOperator
    {
        public EqualityOperatorType OperatorType { get; }

        public EqualityOperator(IExpression left, EqualityOperatorType operatorType, IExpression right) : base(left, right)
        {
            OperatorType = operatorType;
        }
        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
