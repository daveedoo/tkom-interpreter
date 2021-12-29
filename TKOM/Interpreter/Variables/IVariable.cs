using TKOM.Node;

namespace TKOM.Interpreter
{
    public interface IVariable
    {
        public Type Type { get; }
        public string Name { get; }
        public IValue Value { get; }

        public bool TryAssign(IntValue value);
    }
}
