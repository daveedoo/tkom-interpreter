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
                _ => throw new ArgumentException("Unknown variable type.", nameof(type)),
            };
        }
    }
}
