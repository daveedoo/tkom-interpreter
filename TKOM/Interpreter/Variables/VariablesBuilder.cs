using System;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    internal static class VariablesBuilder
    {
        public static IVariable BuildVariable(Type type, string name)
        {
            return type switch
            {
                Type.Int => new IntVariable(name),
                _ => throw new ArgumentException("Invalid variable type.", nameof(type)),
            };
        }
    }
}
