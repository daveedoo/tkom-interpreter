using TKOM.Node;

namespace TKOM.Interpreter
{
    public interface IValue
    {
        public Type Type { get; }
        public bool TryAssignTo(IVariable variable);
    }
}
