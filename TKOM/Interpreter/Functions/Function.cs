using System.Collections.Generic;
using TKOM.Node;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    public abstract class Function
    {
        public Type ReturnType { get; }
        public string Name { get; }
        public IList<Parameter> Parameters { get; }

        public Function(Type returnType, string name, params Parameter[] parameters)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
        }

        public abstract void Accept(Interpreter visitor);
        public bool CanBeCalledLike(string name, IList<Type> argumentTypes)
        {
            if (name != Name ||
                argumentTypes.Count != Parameters.Count)
                return false;
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (argumentTypes[i] != Parameters[i].Type)
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            string paramsStr = "";
            if (Parameters.Count > 0)
            {
                paramsStr = Parameters[0].Type.ToString();
                for (int i = 1; i < Parameters.Count; i++)
                    paramsStr = $"{paramsStr}, {Parameters[i].Type}";
            }
            return $"{ReturnType} {Name}({paramsStr})";
        }
    }
}
