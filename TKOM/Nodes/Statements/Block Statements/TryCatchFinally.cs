using System.Collections.Generic;

namespace TKOM.Node
{
    public class TryCatchFinally : IStatement
    {
        public IStatement TryStatement { get; }
        public IList<Catch> CatchStatements { get; }
        public IStatement FinallyStatement { get; }

        public TryCatchFinally(IStatement tryStatement, IList<Catch> catchStatements, IStatement finallyStatement = null)
        {
            TryStatement = tryStatement;
            CatchStatements = catchStatements;
            FinallyStatement = finallyStatement;
        }

        public void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Catch
    {
        public string Identifier { get; }
        public IStatement Statement { get; }
        public IExpression WhenExpression { get; }

        public Catch(string identifier, IStatement statement, IExpression whenExpression = null)
        {
            Identifier = identifier;
            Statement = statement;
            WhenExpression = whenExpression;
        }
    }
}
