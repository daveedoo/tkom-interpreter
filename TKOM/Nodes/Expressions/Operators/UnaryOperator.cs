namespace TKOM.Node
{
    public enum UnaryOperatorType
    {
        Uminus, LogicalNegation
    }

    public class UnaryOperator : IExpression
    {
        public UnaryOperatorType OperatorType { get; }
        public IExpression Expression { get; }

        public UnaryOperator(UnaryOperatorType operatorType, IExpression expression)
        {
            OperatorType = operatorType;
            Expression = expression;
        }
    }
}
