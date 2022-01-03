using System;
using System.Collections.Generic;
using System.Linq;
using TKOM.ErrorHandler;
using TKOM.Node;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    /// <summary>
    /// When visiting the tree, <see cref="Interpreter"/> assumes its syntactic correctness (e.g. required tree subnodes cannot be <c>null</c>).
    /// </summary>
    public class Interpreter : INodeVisitor
    {
        private Stack<FunctionCallContext> CallStack { get; }
        private IList<FunctionDefinition> Functions { get; }
        private IErrorHandler ErrorHandler { get; }

        private bool error;
        private IValue lastExpressionValue;
        private IValue lastReturnedValue;

        public Interpreter(IErrorHandler errorHandler)
        {
            CallStack = new Stack<FunctionCallContext>();
            Functions = new List<FunctionDefinition>();
            ErrorHandler = errorHandler;

            error = false;
            lastExpressionValue = null;
        }
        public bool ConsumeLastReturnedValue(out IValue value)
        {
            value = lastReturnedValue;
            lastReturnedValue = null;
            return lastReturnedValue is not null;
        }
        public bool ConsumeLastExpressionValue(out IValue value)
        {
            value = lastExpressionValue;
            lastExpressionValue = null;
            return lastExpressionValue is not null;
        }

        private void Error(string message)
        {
            ErrorHandler.Error(message);
            error = true;
        }

        public void Visit(Program program)
        {
            foreach (FunctionDefinition funDef in program.functions)
                Functions.Add(funDef);

            if (!TryFindMain(out FunctionDefinition main))
            {
                Error("Program should contain an entry point (\"main()\" function).");
                return;
            }

            var mainCall = new FunctionCall(main.Name, new List<IExpression>());
            mainCall.Accept(this);
        }
        private bool TryFindMain(out FunctionDefinition main)
        {
            main = null;

            try
            {
                main = Functions.Single(f => f.Name == "main" && f.Parameters.Count == 0);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            return true;
        }

        public void Visit(FunctionDefinition funDef)
        {
            funDef.Body.Accept(this);
        }

        public void Visit(Block block)
        {
            CallStack.Peek().CreateNewScope();

            foreach (IStatement statement in block.Statements)
            {
                statement.Accept(this);
                if (error)
                    return;
            }

            CallStack.Peek().DeleteScope();
        }

        public void Visit(IfStatement ifStatement)
        {
            throw new NotImplementedException();
        }

        public void Visit(TryCatchFinally tryCatchFinally)
        {
            throw new NotImplementedException();
        }

        public void Visit(WhileStatement whileStatement)
        {
            throw new NotImplementedException();
        }

        public void Visit(Assignment assignment)
        {
            if (!CallStack.Peek().TryFindVariable(assignment.VariableName, out IValue value))
            {
                Error($"The variable {assignment.VariableName} does not exist in the current context.");
                return;
            }

            assignment.Expression.Accept(this);
            if (error)
                return;

            if (!ConsumeLastExpressionValue(out IValue varValue) || varValue.Type != value.Type)
            {
                string expType = varValue is null ? "void" : varValue.Type.ToString();
                Error($"Cannot assign value of type {expType} to variable '{assignment.VariableName}' of type {value.Type}.");
                return;
            }

            CallStack.Peek().SetVariable(assignment.VariableName, varValue);
        }

        public void Visit(Declaration declaration)
        {
            if (CallStack.Peek().TryFindVariable(declaration.Name, out _))
            {
                Error($"Redeclaration of variable '{declaration.Name}'.");
                return;
            }

            IValue value = ValuesCreator.CreateValue(declaration.Type);

            CallStack.Peek().AddVariable(declaration.Name, value);
        }

        private bool canFunctionBeCalledLike(FunctionDefinition f, string name, IList<IValue> values)
        {
            if (f.Name != name ||
                f.Parameters.Count != values.Count)
                return false;

            for (int i = 0; i < f.Parameters.Count; i++)
            {
                if (f.Parameters[i].Type != values[i].Type)
                    return false;
            }
            return true;
        }
        private IList<IValue> evaluateExpressions(IList<IExpression> expressions)
        {
            var values = new List<IValue>();

            foreach (IExpression expression in expressions)
            {
                expression.Accept(this);
                ConsumeLastReturnedValue(out IValue value);
                values.Add(value);
            }
            return values;
        }
        private string createCallSignature(string identifier, IList<IValue> values)
        {
            string args = "";
            if (values.Count > 0)
            {
                args = values[0].Type.ToString();
                for (int i = 1; i < values.Count; i++)
                    args = $"{args}, {values[i].Type}";
            }
            return args;
        }
        public void Visit(FunctionCall functionCall)
        {
            IList<IValue> argsValues = evaluateExpressions(functionCall.Arguments);

            var funs = Functions.ToList().FindAll(f =>
                canFunctionBeCalledLike(f, functionCall.Identifier, argsValues));

            if (!funs.Any())
            {
                Error($"No function with signature {createCallSignature(functionCall.Identifier, argsValues)}.");
                return;
            }
            if (funs.Count > 0)
            {
                Error($"Function call ambiguous.");
                return;
            }

            FunctionDefinition function = funs.Single();

            IDictionary<string, IValue> arguments = new Dictionary<string, IValue>();
            for (int i = 0; i < argsValues.Count; i++)
                arguments.Add(function.Parameters[i].Name, argsValues[i]);

            CallStack.Push(new FunctionCallContext(arguments));

            function.Accept(this);

            CallStack.Pop();
        }

        public void Visit(ReturnStatement returnStatement)
        {
            throw new NotImplementedException();
        }

        public void Visit(ThrowStatement throwStatement)
        {
            throw new NotImplementedException();
        }

        public void Visit(Node.Variable variable)
        {
            throw new NotImplementedException();
        }

        public void Visit(IntConst intConst)
        {
            lastExpressionValue = new IntValue(intConst.Value);
        }

        public void Visit(BinaryOperator binaryOperator)
        {
            throw new NotImplementedException();
        }

        public void Visit(UnaryOperator binaryOperator)
        {
            throw new NotImplementedException();
        }
    }
}
