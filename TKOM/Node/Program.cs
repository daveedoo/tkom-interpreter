using System;
using System.Collections.Generic;

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

    public class Block : INode, IEquatable<Block>
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

    public interface IStatement { }
    public record Declaration(Type Type, string Name) : IStatement;

    public enum Type
    {
        Void, IntType
    }
}
