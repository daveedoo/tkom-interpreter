using System.Collections.Generic;
using System.Linq;

namespace TKOM.Interpreter
{
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
