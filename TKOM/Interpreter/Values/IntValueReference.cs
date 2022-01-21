using TKOM.Node;

namespace TKOM.Interpreter
{
    public class IntValueReference : IValueReference
    {
        public Type Type { get; } = Type.Int;
        public int Value { get; set; }
        object IValueReference.Value => Value;

        public IntValueReference(int value)
        {
            Value = value;
        }

        public bool IsEqualTo(int val)
        {
            return Value == val;
        }
        bool IValueReference.IsEqualTo(object val) => IsEqualTo((int)val);

        public IntValueReference Clone()
        {
            return new IntValueReference(Value);
        }
        IValueReference IValueReference.Clone() => Clone();
    }
}
