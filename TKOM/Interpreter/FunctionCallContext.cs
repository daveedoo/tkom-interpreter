using System.Collections.Generic;
using System.Linq;

namespace TKOM.Interpreter
{
    internal class Scope
    {
        private IDictionary<string, IValue> Variables { get; }

        public Scope()
        {
            Variables = new Dictionary<string, IValue>();
        }
        public Scope(IDictionary<string, IValue> initialVariables)
        {
            Variables = initialVariables;
        }

        public void AddVariable(string name, IValue value)
        {
            Variables.Add(name, value);
        }

        public bool RemoveVariable(string name)
        {
            return Variables.Remove(name);
        }

        public bool TryFindVariable(string name, out IValue value)
        {
            return Variables.TryGetValue(name, out value);
        }

        public void SetVariable(string name, IValue value)
        {
            Variables[name] = value;
        }
    }

    internal class FunctionCallContext
    {
        private IList<Scope> Scopes { get; }
        public Function CalledFunction { get; }

        public FunctionCallContext(Function calledFunction, IDictionary<string, IValue> initialVariables)
        {
            Scopes = new List<Scope>
            {
                new Scope(initialVariables)
            };
            CalledFunction = calledFunction;
        }

        public void CreateNewScope()
        {
            Scopes.Add(new Scope());
        }
        public void DeleteScope()
        {
            Scopes.RemoveAt(Scopes.Count - 1);
        }

        public void AddVariable(string name, IValue value)
        {
            Scopes.Last().AddVariable(name, value);
        }

        public bool RemoveVariable(string name)
        {
            return Scopes.Last().RemoveVariable(name);
        }

        public bool TryFindVariable(string name, out IValue variable)
        {
            variable = null;
            foreach (Scope scope in Scopes)
            {
                if (scope.TryFindVariable(name, out variable))
                    return true;
            }
            return false;
        }

        public void SetVariable(string name, IValue value)
        {
            Scope scope = Scopes.Single(s => s.TryFindVariable(name, out _));
            scope.SetVariable(name, value);
        }
    }
}
