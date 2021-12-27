using System.Collections.Generic;
using TKOM.Node;

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

    internal class Variable
    {
        public string Name { get; }
        public IValue Value { get; }

        public Variable(string name, IValue value)
        {
            Name = name;
            Value = value;
        }
    }

    internal interface IValue
    { }

    internal class IntegerValue : IValue
    {
        public int Value { get; }

        public IntegerValue(int value)
        {
            Value = value;
        }
    }

    internal class Scope
    {
        public List<Variable> Variables { get; }

        public Scope()
        {
            Variables = new List<Variable>();
        }
    }

    internal class FunctionCallContext
    {
        public Stack<Scope> Scopes { get; }

        public FunctionCallContext()
        {
            Scopes = new Stack<Scope>();
        }
    }

    internal class Interpreter : INodeVisitor
    {
        public Stack<FunctionCallContext> CallStack { get; }

        public Interpreter()
        {
            CallStack = new Stack<FunctionCallContext>();

            IntegerValue zero = new IntegerValue(0);
            Variable testVar = new Variable("x", zero);
        }

        public void Visit(INode node)
        {
            throw new System.NotImplementedException();
        }

        public void Visit(Program program)
        {

        }
    }
}
