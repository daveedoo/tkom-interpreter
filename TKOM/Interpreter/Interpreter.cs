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
        private FunctionsCollection Functions { get; }

        private bool error;
        private bool returned;              // set by ReturnStatement only
        private IValue lastExpressionValue; // set by all Expressions, only
        public bool ConsumeLastExpressionValue(out IValue value)
        {
            value = lastExpressionValue;
            lastExpressionValue = null;
            return value is not null;
        }


        public Interpreter(IErrorHandler errorHandler, TextWriter stdOut)
        {
            StdOut = stdOut;
            ErrorHandler = errorHandler;
            CallStack = new Stack<FunctionCallContext>();
            Functions = new FunctionsCollection();

            error = false;
            lastExpressionValue = null;

            SetupBuiltinFunctions();
        }
        private void SetupBuiltinFunctions()
        {
            if (!Functions.TryAdd(new PrintFunction(Type.Int)) ||
                !Functions.TryAdd(new PrintFunction(Type.String)))
                throw new Exception($"{nameof(Functions)} already contains a function ambiguous with {nameof(PrintFunction)}");
        }
        private void Error(string message)
        {
            ErrorHandler.Error(message);
            error = true;
        }

        public void Visit(Program program)
        {
            foreach (FunctionDefinition funDef in program.functions)
            {
                var function = new UserFunction(funDef);
                if (!Functions.TryAdd(function))
                {
                    Error($"Program already contains a function ambiguous with {function}");
                    return;
                }
            }
            
            if (!Functions.TryGet("main", new List<Type>(), out Function main))
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

        #region block statements
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
        #endregion
        
        #region Functions
        public void Visit(Function function)
        {
            function.Accept(this);
        }
        public void Visit(PrintFunction _)
        {
            CallStack.Peek().TryFindVariable(PrintFunction.paramName, out IValue value);

            StdOut.Write(value.Value);
        }
        #endregion

        #region Simple statements
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
        private IList<IValue> EvaluateExpressions(params IExpression[] expressions)
        {
            var values = new List<IValue>();

            foreach (IExpression expression in expressions)
            {
                expression.Accept(this);
                if (error)
                    return null;
                ConsumeLastExpressionValue(out IValue value);
                values.Add(value);
            }
            return values;
        }
        private string createCallSignature(string identifier, IList<Type> types)
        {
            string args = "";
            if (types.Count > 0)
            {
                args = types[0].ToString();
                for (int i = 1; i < types.Count; i++)
                    args = $"{args}, {types[i]}";
            }
            return $"{identifier}({args})";
        }
        public void Visit(FunctionCall functionCall)
        {
            IList<IValue> argsValues = EvaluateExpressions(functionCall.Arguments.ToArray()); // TODO: on error
            if (error)
                return;
            IList<Type> argsTypes = argsValues.Select(t => t.Type).ToList();

            if (!Functions.TryGet(functionCall.Identifier, argsTypes, out Function function))
            {
                Error($"No function with signature {createCallSignature(functionCall.Identifier, argsTypes)}.");
                return;
            }

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
        #endregion

        public void Visit(Variable variable)
        {
            if (!CallStack.Peek().TryFindVariable(variable.Identifier, out IValue value))
            {
                Error($"The variable {variable.Identifier} does not exist in the current context.");
                return;
            }

            lastExpressionValue = value;
        }
        public void Visit(IntConst intConst)
        {
            lastExpressionValue = new IntValue(intConst.Value);
        }
        public void Visit(StringConst stringConst)
        {
            lastExpressionValue = new StringValue(stringConst.Value);
        }

        public bool EvaluateBinaryOperator(BinaryOperator binaryOperator, out IValue left, out IValue right)
        {
            left = right = null;
            IList<IValue> values = EvaluateExpressions(binaryOperator.Left, binaryOperator.Right);
            if (error)
                return false;
            left = values[0];
            right = values[1];
            return true;
        }
        public void Visit(LogicalOr logicalOr)
        {
            if (!EvaluateBinaryOperator(logicalOr, out IValue leftValue, out IValue rightValue))
                return;

            if (leftValue is not IntValue || rightValue is not IntValue)
            {
                Error($"Both sides of logical OR expression must be of {Type.Int} type.");
                return;
            }

            if (leftValue.IsEqualTo(0) && rightValue.IsEqualTo(0))
                lastExpressionValue = new IntValue(0);
            else
                lastExpressionValue = new IntValue(1);
        }
        public void Visit(LogicalAnd logicalAnd)
        {
            if (!EvaluateBinaryOperator(logicalAnd, out IValue leftValue, out IValue rightValue))
                return;

            if (leftValue is not IntValue || rightValue is not IntValue)
            {
                Error($"Both sides of logical AND expression must be of {Type.Int} type.");
                return;
            }

            if (leftValue.IsEqualTo(0) || rightValue.IsEqualTo(0))
                lastExpressionValue = new IntValue(0);
            else
                lastExpressionValue = new IntValue(1);
        }
        public void Visit(EqualityOperator equalityOperator)
        {
            if (!EvaluateBinaryOperator(equalityOperator, out IValue leftValue, out IValue rightValue))
                return;

            if (leftValue is not IntValue || rightValue is not IntValue)
            {
                Error($"Both sides of equality expression must be of {Type.Int} type.");
                return;
            }

            int value = equalityOperator.OperatorType switch
            {
                EqualityOperatorType.Equality =>
                    leftValue.IsEqualTo(rightValue.Value) ? 1 : 0,
                EqualityOperatorType.Inequality =>
                    leftValue.IsEqualTo(rightValue.Value) ? 0 : 1,
                _ => throw new ArgumentException("Invalid equality operator type.", nameof(equalityOperator))
            };
            lastExpressionValue = new IntValue(value);
        }
        public void Visit(RelationOperator relationOperator)
        {
            if (!EvaluateBinaryOperator(relationOperator, out IValue leftValue, out IValue rightValue))
                return;

            if (leftValue is not IntValue || rightValue is not IntValue)
            {
                Error($"Both sides of relation expression must be of {Type.Int} type.");
                return;
            }

            int left = (leftValue as IntValue).GetIntValue();
            int right = (rightValue as IntValue).GetIntValue();
            int value = relationOperator.OperatorType switch
            {
                RelationOperatorType.LessEqual => left <= right ? 1 : 0,
                RelationOperatorType.GreaterEqual => left >= right ? 1 : 0,
                RelationOperatorType.LessThan => left < right ? 1 : 0,
                RelationOperatorType.GreaterThan => left < right ? 1 : 0,
                _ => throw new ArgumentException("Invalid relation operator type.", nameof(relationOperator))
            };
            lastExpressionValue = new IntValue(value);
        }
        public void Visit(AdditiveOperator additiveOperator)
        {
            if (!EvaluateBinaryOperator(additiveOperator, out IValue leftValue, out IValue rightValue))
                return;

            if (leftValue is not IntValue || rightValue is not IntValue)
            {
                Error($"Both sides of additive expression must be of {Type.Int} type.");
                return;
            }

            int left = (leftValue as IntValue).GetIntValue();
            int right = (rightValue as IntValue).GetIntValue();
            int value;
            try
            {
                value = additiveOperator.OperatorType switch
                {
                    AdditiveOperatorType.Add => checked(left + right),
                    AdditiveOperatorType.Subtract => checked(left - right),
                    _ => throw new ArgumentException("Invalid additive operator type.", nameof(additiveOperator))
                };
            }
            catch (OverflowException)
            {
                Error($"int overflow.");
                return;
            }

            lastExpressionValue = new IntValue(value);
        }
        public void Visit(MultiplicativeOperator multiplicativeOperator)
        {
            if (!EvaluateBinaryOperator(multiplicativeOperator, out IValue leftValue, out IValue rightValue))
                return;

            if (leftValue is not IntValue || rightValue is not IntValue)
            {
                Error($"Both sides of multiplicative expression must be of {Type.Int} type.");
                return;
            }

            int left = (leftValue as IntValue).GetIntValue();
            int right = (rightValue as IntValue).GetIntValue();
            int value;
            try
            {
                value = multiplicativeOperator.OperatorType switch
                {
                    MultiplicativeOperatorType.Multiply => checked(left * right),
                    MultiplicativeOperatorType.Divide => checked(left / right),
                    _ => throw new ArgumentException("Invalid multiplicative operator type.", nameof(multiplicativeOperator))
                };
            }
            catch (OverflowException)
            {
                Error($"int overflow.");
                return;
            }

            lastExpressionValue = new IntValue(value);
        }
        public void Visit(UnaryOperator unaryOperator)
        {
            unaryOperator.Expression.Accept(this);
            if (error)
                return;

            ConsumeLastExpressionValue(out IValue value);

            if (value is not IntValue)
            {
                Error($"Unary minus expression must be of {Type.Int} type.");
                return;
            }
            int intVal = (value as IntValue).GetIntValue();
            int val = unaryOperator.OperatorType switch
            {
                UnaryOperatorType.Uminus => -intVal,
                UnaryOperatorType.LogicalNegation => intVal == 0 ? 1 : 0,
                _ => throw new ArgumentException("Invalid unary operator type.", nameof(unaryOperator))
            };

            lastExpressionValue = new IntValue(val);
        }
    }
}
