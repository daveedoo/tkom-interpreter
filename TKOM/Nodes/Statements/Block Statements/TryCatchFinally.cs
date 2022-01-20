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
        public string ExceptionVariableName { get; }
        public IStatement Statement { get; }
        public IExpression WhenExpression { get; }

        public Catch(string exceptionVariableName, IStatement statement, IExpression whenExpression = null)
        {
            ExceptionVariableName = exceptionVariableName;
            Statement = statement;
            WhenExpression = whenExpression;
        }
        public Catch(IStatement statement, IExpression whenExpression = null)
        {
            ExceptionVariableName = null;
            Statement = statement;
            WhenExpression = whenExpression;
        }
    }
}
