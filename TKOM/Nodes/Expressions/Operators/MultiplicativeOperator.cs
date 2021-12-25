namespace TKOM.Node
{
    public enum MultiplicativeOperatorType
    {
        Multiply, Divide
    }
    public class MultiplicativeOperator : BinaryOperator
    {
        public MultiplicativeOperatorType OperatorType { get; }

        public MultiplicativeOperator(IExpression left, MultiplicativeOperatorType operatorType, IExpression right) : base(left, right)
        {
            OperatorType = operatorType;
        }
    }
}
