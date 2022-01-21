using System;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    public interface IValueReference
    {
        public Type Type { get; }
        public object Value { get; }
        public bool IsEqualTo(object val);
        public IValueReference Clone();
    }
}
