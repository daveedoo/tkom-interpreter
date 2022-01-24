using TKOM.Node;

namespace TKOM.Interpreter
{
    public class StringValueReference : IValueReference
    {
        public Type Type { get; } = Type.String;
        public string Value { get; set; }
        object IValueReference.Value { get => Value; set { Value = (string)value; } }

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
