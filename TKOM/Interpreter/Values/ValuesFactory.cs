using System;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    public static class ValuesFactory
    {
        public static IValueReference CreateDefaultValue(Type type)
        {
            return type switch
            {
                Type.Int => CreateValue(0),
                Type.String => CreateValue(""),
                _ => throw new ArgumentException($"Cannot instantiate value of type {type.ToString().ToLower()}.", nameof(type)),
            };
        }

        public static IntValueReference CreateValue(int value)
        {
            return new IntValueReference(value);
        }
        public static StringValueReference CreateValue(string value)
        {
            return new StringValueReference(value);
        }
    }
}
