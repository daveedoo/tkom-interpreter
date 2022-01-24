namespace TKOM.Node
{
    public enum AdditiveOperatorType
    {
        Add, Subtract
    }

    public class AdditiveOperator : BinaryOperator
    {
        public AdditiveOperatorType OperatorType { get; }

        public AdditiveOperator(IExpression left, AdditiveOperatorType operatorType, IExpression right) : base(left, right)
        {
            OperatorType = operatorType;
        }
        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
