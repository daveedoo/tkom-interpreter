using System;
using System.Collections.Generic;
using System.IO;
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
        private TextWriter StdOut { get; }
        private IErrorHandler ErrorHandler { get; }
        private Stack<FunctionCallContext> CallStack { get; }
        private IList<Function> Functions { get; }

        private bool error;
        private bool returned;              // set by ReturnStatement only
        private IValue lastExpressionValue; // set by all Expressions, only
        public bool ConsumeLastExpressionValue(out IValue value)
        {
            value = lastExpressionValue;
            lastExpressionValue = null;
            return lastExpressionValue is not null;
        }

        public Interpreter(IErrorHandler errorHandler, TextWriter stdOut)
        {
            CallStack = new Stack<FunctionCallContext>();
            ErrorHandler = errorHandler;
            StdOut = stdOut;
            Functions = new List<Function>
            {
                new PrintFunction()
            };

            error = false;
            lastExpressionValue = null;
        }
        //private void ValidateBuiltinFunctions()
        //{
        //    var funs = BuiltinFunctions.ToList();
        //    for (int i = 0; i < BuiltinFunctions.Count; i++)
        //    {
        //        var paramTypes = funs[i].Parameters.Select(p => p.Type).ToList();
        //        for (int j = i + 1; j < BuiltinFunctions.Count; j++)
        //        {
        //            if (funs[j].CanBeCalledLike(funs[i].Name, paramTypes))
        //                throw new Exception($"{nameof(BuiltinFunctions)} contain functions of types {funs[i].GetType()}, {funs[j].GetType()}" +
        //                    "that can be called in the same way");
        //        }
        //    }
        //}

        private void Error(string message)
        {
            ErrorHandler.Error(message);
            error = true;
        }


        private bool TryFindMain(out Function main)
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
        public void Visit(Program program)
        {
            foreach (FunctionDefinition funDef in program.functions)
                Functions.Add(new UserFunction(funDef));

            if (!TryFindMain(out Function main))
            {
                Error("Program should contain an entry point (\"main()\" function).");
                return;
            }

            var mainCall = new FunctionCall(main.Name, new List<IExpression>());
            mainCall.Accept(this);
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
                if (returned)
                    break;
                if (error)
                    return;
            }

            CallStack.Peek().DeleteScope();
        }

        #region simple statements
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
        #endregion

        private IList<IValue> evaluateExpressions(IList<IExpression> expressions)
        {
            var values = new List<IValue>();

            foreach (IExpression expression in expressions)
            {
                expression.Accept(this);
                ConsumeLastExpressionValue(out IValue value);
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
            return $"{identifier}({args})";
        }

        #region Builtin functions
        public void Visit(Function function)
        {
            function.Accept(this);
        }
        public void Visit(PrintFunction printFunction)
        {
            CallStack.Peek().TryFindVariable(PrintFunction.paramName, out IValue value);
            int? val = (value as IntValue?).Value.Value;
            StdOut.Write(val.Value);
        }
        #endregion

        public void Visit(FunctionCall functionCall)
        {
            IList<IValue> argsValues = evaluateExpressions(functionCall.Arguments);
            IList<Type> argsTypes = argsValues.Select(t => t.Type).ToList();

            var funs = Functions.ToList().FindAll(f => f.CanBeCalledLike(functionCall.Identifier, argsTypes));

            if (!funs.Any())
            {
                Error($"No function with signature {createCallSignature(functionCall.Identifier, argsValues)}.");
                return;
            }
            if (funs.Count > 1)
            {
                Error($"Function call ambiguous between {funs[0]} and {funs[1]}.");
                return;
            }

            Function function = funs.Single();

            IDictionary<string, IValue> arguments = new Dictionary<string, IValue>();
            for (int i = 0; i < argsValues.Count; i++)
                arguments.Add(function.Parameters[i].Name, argsValues[i]);

            CallStack.Push(new FunctionCallContext(arguments));

            function.Accept(this);
            if (error)
                return;
            if (function.ReturnType != Type.Void &&
                (!returned || lastExpressionValue is null))
            {
                Error($"Function should return a value.");
                return;
            }

            CallStack.Pop();
        }

        public void Visit(ReturnStatement returnStatement)
        {
            returnStatement.Expression?.Accept(this);
            returned = true;
        }

        public void Visit(ThrowStatement throwStatement)
        {
            throw new NotImplementedException();
        }

        public void Visit(Variable variable)
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
