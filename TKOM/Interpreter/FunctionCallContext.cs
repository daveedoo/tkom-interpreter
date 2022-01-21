using System.Collections.Generic;
using System.Linq;

namespace TKOM.Interpreter
{
    internal class Variable
    {
        public string Name { get; }
        public IValue Value { get; }

        public Variable(string name, IValue value)
        {
            Name = name;
            Value = value;
        }

        public void ChangeReferencedValue()
        {

        }
    }

    internal class Scope
    {
        private HashSet<Variable> Variables { get; }

        public Scope()
        {
            Variables = new HashSet<Variable>();
        }
        public Scope(params Variable[] initialVariables)
        {
            Variables = new HashSet<Variable>(initialVariables);
        }

        public void AddVariable(Variable variable)
        {
            Variables.Add(variable);
        }

        public bool RemoveVariable(string name)
        {
            return Variables.RemoveWhere(v => v.Name == name) > 0;
        }

        public bool TryFindVariable(string name, out Variable variable)
        {
            IEnumerable<Variable> vars = from v in Variables
                                         where v.Name == name
                                         select v;
            variable = vars.SingleOrDefault();
            return variable is not null;
        }
    }

    internal class FunctionCallContext
    {
        private IList<Scope> Scopes { get; }
        public Function CalledFunction { get; }

        public FunctionCallContext(Function calledFunction, params Variable[] initialVariables)
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

        public void AddVariable(Variable variable)
        {
            Scopes.Last().AddVariable(variable);
        }

        public bool RemoveVariable(string name)
        {
            return Scopes.Last().RemoveVariable(name);
        }

        public bool TryFindVariable(string name, out Variable variable)
        {
            variable = null;
            foreach (Scope scope in Scopes)
            {
                if (scope.TryFindVariable(name, out variable))
                    return true;
            }
            return false;
        }
    }
}
