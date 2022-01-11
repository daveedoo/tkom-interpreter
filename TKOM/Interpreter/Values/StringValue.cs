using TKOM.Node;

namespace TKOM.Interpreter
{
    internal class StringValue : IValue
    {
        public Type Type { get; } = Type.String;
        public object Value { get; }

        public StringValue(string value = "")
        {
            Value = value;
        }

        public bool IsEqualTo(object val)
        {
            return val.Equals(Value);
        }
    }
}
