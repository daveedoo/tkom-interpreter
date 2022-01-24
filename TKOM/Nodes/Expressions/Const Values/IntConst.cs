namespace TKOM.Node
{
    public class IntConst : IExpression
    {
        public int Value { get; }

        public IntConst(int value)
        {
            Value = value;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
