using System.Collections.Generic;
using System.Linq;

namespace TKOM.Interpreter
{
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
}
