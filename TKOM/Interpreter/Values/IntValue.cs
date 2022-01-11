﻿using TKOM.Node;

namespace TKOM.Interpreter
{
    public class IntValue : IValue
    {
        public Type Type { get; } = Type.Int;
        public object Value { get; }

        public IntValue(int value = 0)
        {
            Value = value;
        }

        public bool IsEqualTo(object val)
        {
            return val.Equals(Value);
        }
    }
}
