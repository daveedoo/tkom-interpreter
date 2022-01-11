namespace TKOM.Node
{
    public enum RelationOperatorType
    {
        LessEqual, GreaterEqual,
        LessThan, GreaterThan
    }

    public class RelationOperator : BinaryOperator
    {
        public RelationOperatorType OperatorType { get; }

        public RelationOperator(IExpression left, RelationOperatorType operatorType, IExpression right) : base(left, right)
        {
            OperatorType = operatorType;
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
            throw new System.NotImplementedException();
        }
    }
}
