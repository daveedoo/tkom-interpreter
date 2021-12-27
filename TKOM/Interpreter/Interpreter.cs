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
        public Type ReturnType { get; }
        public string Name { get; }
        public IList<Parameter> Parameters { get; }

        public FunctionSignature(Type type, string name, IList<Parameter> parameters)
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

    public class Interpreter : INodeVisitor
    {
        public Stack<FunctionCallContext> CallStack { get; }
        public IList<FunctionSignature> FunctionSignatures { get; }
        public IErrorHandler ErrorHandler { get; }

        public Interpreter(IErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler;
            CallStack = new Stack<FunctionCallContext>();
            FunctionSignatures = new List<FunctionSignature>();
        }

        public void Visit(Program program)
        {
            foreach (FunctionDefinition funDef in program.functions)
                FunctionSignatures.Add(new FunctionSignature(funDef.ReturnType, funDef.Name, funDef.Parameters.Select(param => new Parameter(param.Type, param.Name)).ToList()));
            
            // check for functions ambiguations
            for (int i = 0; i < FunctionSignatures.Count - 1; i++)
            {
                for (int j = i + 1; j < FunctionSignatures.Count; j++)
                {
                    if (FunctionSignatures[j].Parameters.Count != FunctionSignatures[i].Parameters.Count)
                        continue;
                    bool paramsSame = true;
                    for (int p = 0; p < FunctionSignatures[i].Parameters.Count; p++)
                        if (FunctionSignatures[i].Parameters[p].Type != FunctionSignatures[j].Parameters[p].Type)
                        {
                            paramsSame = false;
                            break;
                        }
                    if (paramsSame && FunctionSignatures[i].Name == FunctionSignatures[j].Name)
                        ErrorHandler.Error($"Function signature ambiguous with {FunctionSignatures[j]}");
                }
            }

            var mains = from f in program.functions
                        where f.Name == "main" && f.Parameters.Count == 0
                        select f;

            FunctionDefinition main;
            try
            {
                main = mains.Single();
            }
            catch (InvalidOperationException)
            {
                ErrorHandler.Error("Program should contain exactly one entry point (\"main()\" function).");
                return;
            }
            main.Accept(this);
        }

        public void Visit(FunctionDefinition funDef)
        {
            return;
        }

        public void Visit(Block block)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(Declaration declaration)
        {
            throw new NotImplementedException();
        }

        public void Visit(FunctionCall functionCall)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
