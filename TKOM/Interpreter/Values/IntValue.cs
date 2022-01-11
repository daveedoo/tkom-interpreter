using TKOM.Node;

namespace TKOM.Interpreter
{
    public class IntValue : IValue
    {
        public Type Type { get; } = Type.Int;
        public int Value { get; }

        public IntValue(int value = 0)
        {
            Value = value;
        }
    }
}
