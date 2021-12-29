using TKOM.Node;

namespace TKOM.Interpreter
{
    public class IntVariable : IVariable
    {
        public Type Type { get; } = Type.Int;
        public string Name { get; }
        public IValue Value { get; private set; }

        public IntVariable(string name)
        {
            Name = name;
        }

        public bool TryAssign(IntValue value)
        {
            Value = value;
            return true;
        }
    }
}
