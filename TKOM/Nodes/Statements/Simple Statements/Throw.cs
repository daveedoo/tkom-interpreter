namespace TKOM.Node
{
    public class Throw : IStatement
    {
        IExpression Expression { get; }

        public Throw(IExpression expression)
        {
            Expression = expression;
        }
    }
}
