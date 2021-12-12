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

        public FunctionDefinition(Type returnType, string name, IList<Parameter> parameters)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
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

    public record Parameter
    {
        public Type Type { get; }
        public string Name { get; }
    }

    public enum Type
    {
        Void, IntType
    }
}
