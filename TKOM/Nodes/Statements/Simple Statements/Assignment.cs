namespace TKOM.Node
{
    public class Assignment : IStatement
    {
        public string VariableName { get; }
        public IExpression Expression { get; }

        public Assignment(string variable, IExpression expression)
        {
            VariableName = variable;
            Expression = expression;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
