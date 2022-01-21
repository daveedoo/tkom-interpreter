using TKOM.Node;

namespace TKOM.Interpreter
{
    public class StringValueReference : IValueReference
    {
        public Type Type { get; } = Type.String;
        public string Value { get; }
        object IValueReference.Value => Value;

        public StringValueReference(string value)
        {
            Value = value;
        }

        public bool IsEqualTo(string val)
        {
            return Value == val;
        }
        bool IValueReference.IsEqualTo(object val) => IsEqualTo((string)val);

        public StringValueReference Clone()
        {
            return new StringValueReference(Value);
        }
        IValueReference IValueReference.Clone() => Clone();
    }
}
