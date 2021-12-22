using System;
using System.Collections.Generic;
using TKOM.Scanner;

namespace TKOM.Node
{
    public class Program : INode, IEquatable<Program>
    {
        public IList<FunctionDefinition> functions { get; }

        public Program(IList<FunctionDefinition> functionDefinitions)
        {
            functions = functionDefinitions;
        }

        public bool Equals(Program other)
        {
            IEnumerator<FunctionDefinition> functionsThis = functions.GetEnumerator();
            IEnumerator<FunctionDefinition> functionsOther = other.functions.GetEnumerator();
            while (functionsThis.MoveNext())
            {
                if (!functionsOther.MoveNext() || functionsThis.Current.Equals(functionsOther.Current))
                    return false;
            }
            return true;
        }
    }

    public class FunctionDefinition : INode, IEquatable<FunctionDefinition>
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

        public bool Equals(FunctionDefinition other)
        {
            if (ReturnType != other.ReturnType || Name != other.Name)
                return false;

            IEnumerator<Parameter> paramsThis = Parameters.GetEnumerator();
            IEnumerator<Parameter> paramsOther = other.Parameters.GetEnumerator();
            while (paramsThis.MoveNext())
            {
                if (!paramsOther.MoveNext() || !paramsThis.Equals(paramsOther))
                    return false;
            }
            return true;
        }
    }

    public record Parameter(Type Type, string Name);

    public class Block : INode, IStatement, IEquatable<Block>
    {
        public IList<IStatement> Statements { get; }

        public Block(IList<IStatement> statements)
        {
            Statements = statements;
        }

        public bool Equals(Block other)
        {
            IEnumerator<IStatement> statementsThis = Statements.GetEnumerator();
            IEnumerator<IStatement> statementsOther = other.Statements.GetEnumerator();
            while (statementsThis.MoveNext())
            {
                if (!statementsOther.MoveNext())
                    return false;
            }
            return true;
        }
    }

    public interface IExpression { }
    public interface IStatement : INode { }
    public record Declaration(Type Type, string Name) : IStatement;
    public record Assignment(string Variable, IExpression Expression) : IStatement, IExpression;
    public record Return(IExpression Expression = null) : IStatement;
    public record Throw(IExpression Expression) : IStatement;
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
    
    public record IntConst(int Value) : IExpression;
    public record Variable(string Identifier) : IExpression;

    public record If (IExpression Condition, IStatement IfStatement, IStatement ElseStatement = null) : IStatement;
    public record While(IExpression condition, IStatement Statement) : IStatement;
    public class TryCatchFinally : IStatement, IEquatable<TryCatchFinally>
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

        public bool Equals(TryCatchFinally other)
        {
            if (!TryStatement.Equals(other.TryStatement) ||
                !FinallyStatement.Equals(other.FinallyStatement))
                return false;
            IEnumerator<Catch> statementsThis = CatchStatements.GetEnumerator();
            IEnumerator<Catch> statementsOther = other.CatchStatements.GetEnumerator();
            while (statementsThis.MoveNext())
            {
                if (!statementsOther.MoveNext())
                    return false;
            }
            return true;
        }
    }
    public record Catch(string Identifier, IStatement Statement, IExpression WhenExpression = null) : IStatement;

    // OPERATORS
    public record LogicalOr(IExpression Expression1, IExpression Expression2) : IExpression;
    public record LogicalAnd(IExpression Expression1, IExpression Expression2) : IExpression;
    public record EqualityComparer(IExpression Expression1, EqualityComparerType EqualityComparerType, IExpression Expression2) : IExpression;
    public record RelationOperator(IExpression Expression1, RelationType Relation, IExpression Expression2) : IExpression;
    public record Additive(IExpression Expression1, AdditiveOperator Operator, IExpression Expression2) : IExpression;
    public record Multiplicative(IExpression Expression1, MultiplicativeOperator Operator, IExpression Expression2) : IExpression;
    public record Unary(UnaryOperator Operator, IExpression Expression) : IExpression;

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
