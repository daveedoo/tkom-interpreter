using System;
using System.Collections.Generic;
using System.Linq;
using TKOM.ErrorHandler;
using TKOM.Node;
using Type = TKOM.Node.Type;

namespace TKOM.Interpreter
{
    public class FunctionSignature
    {
        public Type? ReturnType { get; }    // null == void
        public string Name { get; }
        public IList<Parameter> Parameters { get; }

        public FunctionSignature(Type? type, string name, IList<Parameter> parameters)
        {
            ReturnType = type;
            Name = name;
            Parameters = parameters;
        }

        public override string ToString()
        {
            string parametersStr = "";
            if (Parameters.Count > 0)
                parametersStr = $"{Parameters[0]}";
            for (int i = 1; i < Parameters.Count; i++)
                parametersStr = $"{parametersStr}, {Parameters[i]}";

            return $"{ReturnType} {Name}({parametersStr})";
        }


    }
    public class Parameter
    {
        public Type Type { get; }
        public string Name { get; }

        public Parameter(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }

    public static class TypeExtensions
    {
        //public static IntVariable CreateVariable(this Type type, string name)
        //{
        //    return type switch
        //    {
        //        Type.IntType => new IntVariable(name, new IntValue(0)),
        //        _ => throw new ArgumentException("Invalid variable type.", nameof(type)),
        //    };
        //}
    }

    /// <summary>
    /// When visiting the tree, <see cref="Interpreter"/> assumes its syntactic correctness (e.g. required tree subnodes cannot be <c>null</c>).
    /// </summary>
    public class Interpreter : INodeVisitor
    {
        private Stack<FunctionCallContext> CallStack { get; }
        private IList<FunctionSignature> FunctionSignatures { get; }
        private IErrorHandler ErrorHandler { get; }

        private bool error;
        private IValue lastExpressionValue;


        public Interpreter(IErrorHandler errorHandler)
        {
            CallStack = new Stack<FunctionCallContext>();
            FunctionSignatures = new List<FunctionSignature>();
            ErrorHandler = errorHandler;
            error = false;
        }

        private void Error(string message)
        {
            ErrorHandler.Error(message);
            error = true;
        }

        public void Visit(Program program)
        {
            if (!SemanticCheck(program, out FunctionDefinition main))
                return;

            foreach (FunctionDefinition funDef in program.functions)
                FunctionSignatures.Add(new FunctionSignature(funDef.ReturnType, funDef.Name, funDef.Parameters.Select(param => new Parameter(param.Type, param.Name)).ToList()));

            main.Accept(this);
        }
        private bool SemanticCheck(Program program, out FunctionDefinition main)
        {
            main = null;
            IList<FunctionDefinition> functions = program.functions;

            // functions ambiguations
            for (int i = 0; i < functions.Count - 1; i++)
            {
                for (int j = i + 1; j < functions.Count; j++)
                {
                    if (functions[j].Parameters.Count != functions[i].Parameters.Count)
                        continue;
                    bool paramsSame = true;
                    for (int p = 0; p < functions[i].Parameters.Count; p++)
                        if (functions[i].Parameters[p].Type != functions[j].Parameters[p].Type)
                        {
                            paramsSame = false;
                            break;
                        }
                    if (paramsSame && functions[i].Name == functions[j].Name)
                    {
                        Error($"Program already defines function called '{functions[j].Name}' with the same parameter types.");
                        return false;
                    }
                }
            }

            // correct main
            try
            {
                main = functions.Single(f => f.Name == "main" && f.Parameters.Count == 0);
            }
            catch (InvalidOperationException)
            {

                Error("Program should contain an entry point (\"main()\" function).");
                return false;
            }
            return true;
        }

        public void Visit(FunctionDefinition funDef)
        {
            if (!SemanticCheck(funDef))
                return;
            CallStack.Push(new FunctionCallContext());

            funDef.Body.Accept(this);

            CallStack.Pop();
        }
        private bool SemanticCheck(FunctionDefinition funDef)
        {
            if (funDef.ReturnType is not null &&
                !funDef.Body.Statements.Any(s => s is ReturnStatement))
            {
                Error($"Function {funDef.Name} should return a value.");
                return false;
            }
            return true;
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
            if (!CallStack.Peek().TryFindVariable(assignment.VariableName, out IVariable variable))
            {
                Error($"The variable {assignment.VariableName} does not exist in the current context.");
                return;
            }

            assignment.Expression.Accept(this);
            if (error)
                return;

            string expType = lastExpressionValue is null ? "void" : lastExpressionValue.Type.ToString();
            if (lastExpressionValue is null || !lastExpressionValue.TryAssignTo(variable))
                Error($"Cannot assign value of type {expType} to variable '{variable.Name}' of type {variable.Type}.");
        }

        public void Visit(Declaration declaration)
        {
            if (CallStack.Peek().TryFindVariable(declaration.Name, out _))
            {
                Error($"Redeclaration of variable '{declaration.Name}'");
                return;
            }

            IVariable variable = VariablesBuilder.BuildVariable(declaration.Type, declaration.Name);

            CallStack.Peek().AddVariable(variable);
        }

        public void Visit(FunctionCall functionCall)
        {
            //var funs = from f in FunctionSignatures
            //           where f.Name == functionCall.Identifier
            //           && f.Parameters.Count == functionCall.Arguments.Count
            //var functions = FunctionSignatures.Join()


            //FunctionSignatures.Single(f =>
            //{
            //    if (f.Name != functionCall.Identifier)
            //        return false;
            //    if (f.Parameters.Count != functionCall.Arguments.Count)
            //        return false;

            //    for (int i = 0; i < f.Parameters.Count; i++)
            //    {
            //        //if (f.Parameters[i].Type != functionCall.Arguments[i].Type)   // TODO: trzeba dodać atrybuty
            //            //return false;
            //        if (functionCall.Arguments[i].)
            //    }
            //    return true;
            //});
            //throw new NotImplementedException();
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
