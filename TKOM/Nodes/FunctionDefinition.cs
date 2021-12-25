using System;
using System.Collections.Generic;

namespace TKOM.Node
{
    public class FunctionDefinition : INode
    {
        public Type ReturnType { get; }
        public string Name { get; }
        public IList<Parameter> Parameters { get; }
        public Block Body { get; }

        public FunctionDefinition(Type returnType, string name, IList<Parameter> parameters, Block body)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
    public class Parameter
    {
        public Type Type { get; }
        public string Name { get; }

        public Parameter(Type type, string name)
        {
            if (type == Type.Void)
                throw new ArgumentException("Parameter cannot be of void type.");

            Type = type;
            Name = name;
        }
    }
}
