using TKOM.Node;

namespace TKOM.Interpreter
{
    public class IntValue : IValue
    {
        public Type Type { get; } = Type.Int;
        public object Value { get; private set; }

        public IntValue(int value = 0)
        {
            Value = value;
        }

        public bool IsEqualTo(object val)
        {
            return val.Equals(Value);
        }

        public int GetIntValue()
        {
            return (Value as int?).Value;
        }

        public void SetIntValue(int newValue)
        {
            Value = newValue;
        }
    }
}
