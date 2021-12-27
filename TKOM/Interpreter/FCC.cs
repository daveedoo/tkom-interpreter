using System.Collections.Generic;

namespace TKOM.Interpreter
{
    //internal class SymbolsTable
    //{
    //    public HashSet<ISymbol> Symbols { get; }
    //}

    //internal interface ISymbol
    //{
    //    //public Type Type { get; }
    //    public string Name { get; }
    //}

    //internal class IntVariable : ISymbol
    //{
    //    public string Name { get; }
    //    public int? Value { get; }
    //}

    //internal class Function : ISymbol
    //{
    //    public Type Type { get; }
    //    public string Name { get; }
    //    public IList<Parameter> Parameters { get; }
    //}

    public class Variable
    {
        public string Name { get; }
        public IValue Value { get; }

        public Variable(string name, IValue value)
        {
            Name = name;
            Value = value;
        }
    }

    public interface IValue
    { }

    internal class IntegerValue : IValue
    {
        public int Value { get; }

        public IntegerValue(int value)
        {
            Value = value;
        }
    }

    public class Scope
    {
        public List<Variable> Variables { get; }

        public Scope()
        {
            Variables = new List<Variable>();
        }
    }

    public class FunctionCallContext
    {
        public Stack<Scope> Scopes { get; }

        public FunctionCallContext()
        {
            Scopes = new Stack<Scope>();
        }
    }
}
