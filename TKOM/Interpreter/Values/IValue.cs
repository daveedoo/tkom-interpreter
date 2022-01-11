using TKOM.Node;

namespace TKOM.Interpreter
{
    public interface IValue
    {
        public Type Type { get; }
        public object Value { get; }
        public bool IsEqualTo(object val);
    }
}
