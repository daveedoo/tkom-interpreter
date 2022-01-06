using TKOM.Node;

namespace TKOM.Interpreter
{
    public class UserFunction : Function
    {
        private FunctionDefinition FunctionDefinition { get; }

        public UserFunction(FunctionDefinition function) : base(function.ReturnType, function.Name, function.Parameters)
        {
            FunctionDefinition = function;
        }

        public override void Accept(Interpreter visitor)
        {
            FunctionDefinition.Accept(visitor);
        }
    }
}
