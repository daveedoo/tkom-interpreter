using TKOM.Node;

namespace TKOM.Interpreter
{
    public struct IntValue : IValue
    {
        public Type Type { get; } = Type.Int;
        public int? Value { get; }

        public IntValue(int value)
        {
            Value = value;
        }
    }
}
