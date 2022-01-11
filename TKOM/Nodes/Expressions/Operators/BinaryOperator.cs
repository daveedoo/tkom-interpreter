namespace TKOM.Node
{
    public abstract class BinaryOperator : IExpression
    {
        public IExpression Left { get; }
        public IExpression Right { get; }

        public BinaryOperator(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }

        public abstract void Accept(INodeVisitor visitor);
    }
}
