using TKOM.Node;

namespace TKOM.Interpreter
{
    internal class Variable
    {
        public Type Type => ValueReference.Type;
        public string Name { get; }
        public IValueReference ValueReference { get; set; }

        public Variable(string name, IValueReference value)
        {
            Name = name;
            ValueReference = value;
        }
    }
}
