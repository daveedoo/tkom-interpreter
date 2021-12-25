namespace TKOM.Node
{
    public class Assignment : IStatement
    {
        public string Variable { get; }
        public IExpression Expression { get; }

        public Assignment(string variable, IExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }
    }
}
