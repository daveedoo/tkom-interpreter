namespace TKOM.Node
{
    public class Variable : IExpression
    {
        public string Identifier { get; }

        public Variable(string identifier)
        {
            Identifier = identifier;
        }
    }
}
