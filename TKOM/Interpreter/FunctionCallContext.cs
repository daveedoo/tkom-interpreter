using System.Collections.Generic;

namespace TKOM.Interpreter
{
    internal class Scope
    {
        public IList<IVariable> Variables { get; }

        public Scope()
        {
            Variables = new List<IVariable>();
        }

        public void AddVariable(IVariable variable)
        {
            Variables.Add(variable);
        }

        public bool TryFindVariable(string name, out IVariable variable)
        {
            for (int i = 0; i < Variables.Count; i++)
                if (Variables[i].Name == name)
                {
                    variable = Variables[i];
                    return true;
                }

            variable = null;
            return false;
        }
    }

    internal class FunctionCallContext
    {
        private Stack<Scope> Scopes { get; }

        public FunctionCallContext()
        {
            Scopes = new Stack<Scope>();
        }

        public void CreateNewScope()
        {
            Scopes.Push(new Scope());
        }
        public void DeleteScope()
        {
            Scopes.Pop();
        }

        public void AddVariable(IVariable variable)
        {
            Scopes.Peek().AddVariable(variable);
        }

        public bool TryFindVariable(string name, out IVariable variable)
        {
            variable = null;
            Stack<Scope> stack = new Stack<Scope>();

            while (variable is null && Scopes.TryPop(out Scope scope))
            {
                scope.TryFindVariable(name, out variable);
                stack.Push(scope);
            }

            while (stack.TryPop(out Scope scope))
                Scopes.Push(scope);

            return variable is not null;
        }
    }
}
