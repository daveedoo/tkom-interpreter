using System.Collections.Generic;

namespace TKOM.Node
{
    public class Program : INode
    {
        public IList<FunctionDefinition> functions { get; }

        public Program(IList<FunctionDefinition> functionDefinitions)
        {
            functions = functionDefinitions;
        }
    }

    public class FunctionDefinition : INode
    {
        public Type ReturnType { get; }
        public string Name { get; }
        public IList<Parameter> Parameters { get; }
        public Block Body { get; }

        public FunctionDefinition(Type returnType, string name, IList<Parameter> parameters, Block body)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }

    public class Parameter
    {
        public Type Type { get; }
        public string Name { get; }

        public Parameter(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class Block : INode, IStatement
    {
        public IList<IStatement> Statements { get; }

        public Block(IList<IStatement> statements)
        {
            Statements = statements;
        }
    }

    public interface IExpression { }
    public interface IStatement : INode { }
    public class Declaration : IStatement
    {
        public Type Type { get; }
        public string Name { get; }
        public Declaration(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }
    public class Assignment : IStatement, IExpression
    {
        public string Variable { get; }
        public IExpression Expression { get;}

        public Assignment(string variable, IExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }
    }
    public class Return : IStatement
    {
        public IExpression Expression { get; }

        public Return(IExpression expression = null)
        {
            Expression = expression;
        }
    }
    public class Throw : IStatement
    {
        IExpression Expression { get; }

        public Throw(IExpression expression)
        {
            Expression = expression;
        }
    }
    public class FunctionCall : IStatement, IExpression
    {
        public string Identifier { get; set; }
        public IList<IExpression> Arguments { get; }

        public FunctionCall(string identifier, IList<IExpression> arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }
    }
    
    public class IntConst : IExpression
    {
        public int Value { get; }

        public IntConst(int value)
        {
            Value = value;
        }
    }
    public class Variable : IExpression
    {
        public string Identifier { get; }

        public Variable(string identifier)
        {
            Identifier = identifier;
        }
    }

    public class If : IStatement
    {
        public IExpression Condition { get; }
        public IStatement IfStatement { get; }
        public IStatement ElseStatement { get; }

        public If(IExpression condition, IStatement ifStatement, IStatement elseStatement = null)
        {
            Condition = condition;
            IfStatement = ifStatement;
            ElseStatement = elseStatement;
        }
    }

    public class While : IStatement
    {
        public IExpression condition { get; }
        public IStatement Statement { get; }

        public While(IExpression condition, IStatement statement)
        {
            this.condition = condition;
            Statement = statement;
        }
    }
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
    }
    public class Catch : IStatement
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

    // OPERATORS
    public abstract class BinaryOperator : IExpression
    {
        public IExpression Left { get; }
        public IExpression Right { get; }

        public BinaryOperator(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }
    }
    public class LogicalOr : BinaryOperator
    {
        public LogicalOr(IExpression left, IExpression right) : base(left, right)
        { }
    }
    public class LogicalAnd : BinaryOperator
    {
        public LogicalAnd(IExpression left, IExpression right) : base(left, right)
        { }
    }
    public class EqualityComparer : BinaryOperator
    {
        public EqualityComparerType ComparerType { get; }

        public EqualityComparer(IExpression left, EqualityComparerType comparerType, IExpression right) : base(left, right)
        {
            ComparerType = comparerType;
        }
    }

    public class RelationOperator : BinaryOperator
    {
        public RelationType RelationType { get; }

        public RelationOperator(IExpression left, RelationType relationType, IExpression right) : base(left, right)
        {
            RelationType = relationType;
        }
    }

    public class Additive : BinaryOperator
    {
        public AdditiveOperator AdditiveOperator { get; }
        
        public Additive(IExpression left, AdditiveOperator additiveOperator, IExpression right) : base(left, right)
        {
            AdditiveOperator = additiveOperator;
        }
    }

    public class Multiplicative : BinaryOperator
    {
        public MultiplicativeOperator MultiplicativeOperator { get; }
        
        public Multiplicative(IExpression left, MultiplicativeOperator multiplicativeOperator, IExpression right) : base(left, right)
        {
            MultiplicativeOperator = multiplicativeOperator;
        }
    }

    public class Unary : IExpression
    {
        public UnaryOperator Operator { get; }
        public IExpression Expression { get; }

        public Unary(UnaryOperator @operator, IExpression expression)
        {
            Operator = @operator;
            Expression = expression;
        }
    }

    public enum Type
    {
        Void, IntType
    }
    public enum RelationType
    {
        LessEqual, GreaterEqual,
        LessThan, GreaterThan
    }
    public enum EqualityComparerType
    {
        Equality, Inequality
    }
    public enum AdditiveOperator
    {
        Add, Subtract
    }
    public enum MultiplicativeOperator
    {
        Multiply, Divide
    }
    public enum UnaryOperator
    {
        Uminus, LogicalNegation
    }
}
