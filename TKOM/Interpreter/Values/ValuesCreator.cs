using System;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    internal static class ValuesCreator
    {
        public static IValue CreateValue(Type type)
        {
            return type switch
            {
                Type.Int => new IntValue(),
                Type.String => new StringValue(),
                _ => throw new ArgumentException("Invalid variable type.", nameof(type)),
            };
        }
    }
}
