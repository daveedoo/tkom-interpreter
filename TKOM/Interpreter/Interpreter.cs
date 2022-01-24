using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private TextReader StdIn { get; }
        private IErrorHandler ErrorHandler { get; }
        private Stack<FunctionCallContext> CallStack { get; }
        private FunctionsCollection Functions { get; }

        private bool error;
        private bool returned;          // set by ReturnStatement only
        private bool thrown;            // set by ThrownStatement only
        private bool looping;           // set by WhileStatement only
        private bool loopBroken;        // set by BreakStatement only
        private IValueReference lastExpressionValue; // set by all Expressions, only
        private int functionsThrownBy;
        private static readonly string exceptionVariableName = "$exception";
        private static readonly string printVariableName = "$print";
        private static readonly string readVariableName = "$read";
        public bool ConsumeLastExpressionValue(out IValueReference value)
        {
            value = lastExpressionValue;
            lastExpressionValue = null;
            return value is not null;
        }


        public Interpreter(IErrorHandler errorHandler, TextWriter stdOut, TextReader stdIn)
        {
            StdIn = stdIn;
            StdOut = stdOut;
            ErrorHandler = errorHandler;
            CallStack = new Stack<FunctionCallContext>();
            Functions = new FunctionsCollection();

            SetupBuiltinFunctions();
        }
        private void SetupBuiltinFunctions()
        {
            Functions.Add(new PrintFunction(Type.Int, printVariableName));
            Functions.Add(new PrintFunction(Type.String, printVariableName));
            Functions.Add(new ReadFunction(Type.Int, readVariableName));
        }
        private void Error(string message)
        {
            ErrorHandler.Error(message);
            error = true;
        }
        private void PrintCallStack()
        {
            Stack<FunctionCallContext> helpStack = new();
            while (CallStack.Any())
            {
                FunctionCallContext fcc = CallStack.Pop();
                Console.WriteLine($"{fcc.CalledFunction}");
                helpStack.Push(fcc);
            }
            while (helpStack.Any())
            {
                CallStack.Push(helpStack.Pop());
            }
        }

        private void ResetState()
        {
            error = false;
            returned = false;
            thrown = false;
            looping = false;
            loopBroken = false;
            lastExpressionValue = null;
            functionsThrownBy = 0;
        }
        public void Interpret(Program ast)
        {
            ResetState();
            ast.Accept(this);
        }

        #region visitors
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

            if (error || thrown)
                PrintCallStack();
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
                if (returned || loopBroken)
                    break;
                if (thrown || error)
                    return;
            }

            CallStack.Peek().DeleteScope();
        }

        #region block statements
        public void Visit(IfStatement ifStatement)
        {
            if (!EvaluateCondition(ifStatement.Condition, out int conditionValue))
                return;

            if (conditionValue != 0)
            {
                if (ifStatement.TrueStatement is Declaration)
                {
                    Error($"Embedded statement cannot be a declaration.");
                    return;
                }
                ifStatement.TrueStatement.Accept(this);
            }
            else if (ifStatement.ElseStatement is not null)
            {
                if (ifStatement.ElseStatement is Declaration)
                {
                    Error($"Embedded statement cannot be a declaration.");
                    return;
                }
                ifStatement.ElseStatement.Accept(this);
            }
        }
        /// <returns>Information if the statement was visited successfully.</returns>
        private bool TryVisitCatchEmbeddedStatement(Catch catchStatement)
        {
            if (catchStatement.Statement is Declaration)
            {
                Error($"Embedded statement cannot be a declaration.");
                return false;
            }
            catchStatement.Statement.Accept(this);
            if (error)
                return false;
            if (catchStatement.ExceptionVariableName is not null)
                CallStack.Peek().RemoveVariable(catchStatement.ExceptionVariableName);

            // take off the stack all the functions thrown by due to the exception throw
            while (functionsThrownBy > 0)
            {
                CallStack.Pop();
                functionsThrownBy--;
            }
            return true;
        }
        /// <summary>
        /// Tries to visit catch block.
        /// </summary>
        /// <param name="catchBlock"></param>
        /// <returns>Information, if the exception was caught by this <paramref name="catchBlock"/>.</returns>
        /// <remarks>Can set <see cref="thrown"/> or <see cref="error"/> flags. </remarks>
        private bool TryVisitCatchBlock(Catch catchBlock)
        {
            if (catchBlock.ExceptionVariableName is not null)
            {
                CallStack.Peek().TryFindVariable(exceptionVariableName, out Variable exceptionVariable);
                CallStack.Peek().AddVariable(new Variable(catchBlock.ExceptionVariableName, exceptionVariable.ValueReference));
            }

            if (catchBlock.WhenExpression is null)
                return TryVisitCatchEmbeddedStatement(catchBlock);

            catchBlock.WhenExpression.Accept(this);
            if (thrown || error)
            {
                thrown = false; // ignores the exception thrown during "when" evaluation
                return false;
            }

            ConsumeLastExpressionValue(out IValueReference value);
            if (value.Type != Type.Int)
            {
                Error($"Expression of invalid type, needs to be {Type.Int}.");
                return false;
            }

            int conditionValue = (value as IntValueReference).Value;
            if (conditionValue != 0)
                return TryVisitCatchEmbeddedStatement(catchBlock);
            return false;
        }
        public void Visit(TryCatchFinally tcf)
        {
            if (tcf.TryStatement is Declaration)
            {
                Error($"Embedded statement cannot be a declaration.");
                return;
            }
            tcf.TryStatement.Accept(this);
            if (error)
                return;

            if (thrown)
            {
                thrown = false;
                bool caught = false;
                foreach (var catchStmt in tcf.CatchStatements)
                {
                    caught = TryVisitCatchBlock(catchStmt);
                    if (error)
                        return;
                    if (caught)
                    {
                        //thrown = false;
                        break;
                    }
                }
                if (caught)
                    CallStack.Peek().RemoveVariable(exceptionVariableName);
                else
                    thrown = true;
            }
            if (tcf.FinallyStatement is Declaration)
            {
                Error($"Embedded statement cannot be a declaration.");
                return;
            }
            tcf.FinallyStatement?.Accept(this);
        }
        private bool EvaluateCondition(IExpression condition, out int conditionValue)
        {
            conditionValue = 0;

            condition.Accept(this);
            if (error)
                return false;
            ConsumeLastExpressionValue(out IValueReference val);
            if (val.Type != Type.Int)
            {
                Error($"Condition statement should be of {Type.Int} type.");
                return false;
            }
            conditionValue = (val as IntValueReference).Value;
            return true;
        }
        public void Visit(WhileStatement whileStatement)
        {
            if (whileStatement.Statement is Declaration)
            {
                Error($"Embedded statement cannot be a declaration.");
                return;
            }

            if (!EvaluateCondition(whileStatement.Condition, out int conditionValue))
                return;

            if (conditionValue != 0)
                looping = true;
            while (conditionValue != 0)
            {
                whileStatement.Statement.Accept(this);
                if (thrown || error || returned)
                    break;
                if (loopBroken)
                {
                    loopBroken = false;
                    break;
                }

                if (!EvaluateCondition(whileStatement.Condition, out conditionValue))
                    return;
            }
            looping = false;
        }
        public void Visit(BreakStatement breakStatement)
        {
            if (!looping)
            {
                Error("No enclosing loop out of which to break.");
                return;
            }
            loopBroken = true;
        }
        #endregion

        #region Functions
        public void Visit(Function function)
        {
            function.Accept(this);
        }
        public void Visit(PrintFunction p)
        {
            CallStack.Peek().TryFindVariable(p.Parameters[0].Name, out Variable variable);

            StdOut.Write(variable.ValueReference.Value);
        }

        public void Visit(ReadFunction readFunction)
        {
            CallStack.Peek().TryFindVariable(readFunction.Parameters[0].Name, out Variable variable);
            if (variable.Type != Type.Int)
            {
                Error($"Cannot read variable of type different than {Type.Int.ToString().ToLower()}");
                return;
            }

            StringBuilder readNumber = new();
            int readChar = StdIn.Read();
            while (char.IsWhiteSpace((char)readChar))
                readChar = StdIn.Read();
            while (char.IsDigit((char)readChar))
            {
                readNumber.Append((char)readChar);
                readChar = StdIn.Read();
            }
            if (!int.TryParse(readNumber.ToString(), out int readValue))
            {
                Error("Invalid input. Onli digit characters are accepted.");
                return;
            }
            (variable.ValueReference as IntValueReference).Value = readValue;
        }
        #endregion

        #region Simple statements
        public void Visit(Assignment assignment)
        {
            if (!CallStack.Peek().TryFindVariable(assignment.VariableName, out Variable variable))
            {
                Error($"The variable {assignment.VariableName} does not exist in the current context.");
                return;
            }

            assignment.Expression.Accept(this);
            if (error)
                return;

            if (!ConsumeLastExpressionValue(out IValueReference rhsValueRef) || rhsValueRef.Type != variable.Type)
            {
                string expType = rhsValueRef is null ? "void" : rhsValueRef.Type.ToString();
                Error($"Cannot assign value of type {expType} to variable '{assignment.VariableName}' of type {variable.Type}.");
                return;
            }

            variable.ValueReference.Value = rhsValueRef.Value;
        }
        public void Visit(Declaration declaration)
        {
            if (CallStack.Peek().TryFindVariable(declaration.Name, out _))
            {
                Error($"Redeclaration of variable '{declaration.Name}'.");
                return;
            }

            IValueReference value = ValuesFactory.CreateDefaultValue(declaration.Type);

            CallStack.Peek().AddVariable(new Variable(declaration.Name, value));
        }
        private IList<IValueReference> EvaluateExpressions(params IExpression[] expressions)
        {
            var values = new List<IValueReference>();

            foreach (IExpression expression in expressions)
            {
                expression.Accept(this);
                if (thrown)
                    return values;
                if (error)
                    return null;
                ConsumeLastExpressionValue(out IValueReference value);
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
            IList<IValueReference> argsValueReferences = EvaluateExpressions(functionCall.Arguments.ToArray());
            if (thrown || error)
                return;
            IList<Type> argsTypes = argsValueReferences.Select(t => t.Type).ToList();

            if (!Functions.TryGet(functionCall.Identifier, argsTypes, out Function function))
            {
                Error($"No function with signature {createCallSignature(functionCall.Identifier, argsTypes)}.");
                return;
            }

            IList<Variable> arguments = new List<Variable>();
            for (int i = 0; i < argsValueReferences.Count; i++)
                arguments.Add(new Variable(function.Parameters[i].Name, argsValueReferences[i]));

            bool returnLooping = looping;
            looping = false;
            CallStack.Push(new FunctionCallContext(function, arguments.ToArray()));

            function.Accept(this);
            if (thrown)
            {
                functionsThrownBy++;
                return;
            }
            if (error)
                return;
            
            if ((function.ReturnType != Type.Void) &&
                (!returned || lastExpressionValue is null))
            {
                Error($"Function should return a value.");
                return;
            }
            returned = false;
            looping = returnLooping;

            CallStack.Pop();
        }
        public void Visit(ReturnStatement returnStatement)
        {
            returnStatement.Expression?.Accept(this);
            returned = true;
        }
        /// <summary>
        /// If error is encountered during visiting <paramref name="throwStatement"/>, exception is not thrown.<br></br>
        /// If exception is thrown during visiting <paramref name="throwStatement"/>'s expression, only "inner" exception is thrown.
        /// </summary>
        public void Visit(ThrowStatement throwStatement)
        {
            throwStatement.Expression.Accept(this);
            if (thrown || error)
                return;
            ConsumeLastExpressionValue(out IValueReference value);
            if (value.Type != Type.Int)
            {
                Error($"Cannot throw value of type different than {Type.Int}.");
                return;
            }
            CallStack.Peek().AddVariable(new Variable(exceptionVariableName, value));
            thrown = true;
        }
        #endregion

        public void Visit(Node.Variable variable)
        {
            if (!CallStack.Peek().TryFindVariable(variable.Identifier, out Variable var))
            {
                Error($"The variable {variable.Identifier} does not exist in the current context.");
                return;
            }

            lastExpressionValue = var.ValueReference;
        }
        public void Visit(IntConst intConst)
        {
            lastExpressionValue = new IntValueReference(intConst.Value);
        }
        public void Visit(StringConst stringConst)
        {
            lastExpressionValue = new StringValueReference(stringConst.Value);
        }

        #region Operators
        public bool EvaluateBinaryOperator(BinaryOperator binaryOperator, out IValueReference left, out IValueReference right)
        {
            left = right = null;
            IList<IValueReference> values = EvaluateExpressions(binaryOperator.Left, binaryOperator.Right);
            if (error)
                return false;
            left = values[0];
            right = values[1];
            if (left is null || right is null)
            {
                Error($"None side of binary expression can be of {Type.Void.ToString().ToLower()} type.");
                return false;
            }
            return true;
        }
        public void Visit(LogicalOr logicalOr)
        {
            if (!EvaluateBinaryOperator(logicalOr, out IValueReference leftValue, out IValueReference rightValue))
                return;

            if (leftValue.Type != Type.Int || rightValue.Type != Type.Int)
            {
                Error($"Both sides of logical OR expression must be of {Type.Int} type.");
                return;
            }

            if (leftValue.IsEqualTo(0) && rightValue.IsEqualTo(0))
                lastExpressionValue = ValuesFactory.CreateValue(0);
            else
                lastExpressionValue = ValuesFactory.CreateValue(1);
        }
        public void Visit(LogicalAnd logicalAnd)
        {
            if (!EvaluateBinaryOperator(logicalAnd, out IValueReference leftValue, out IValueReference rightValue))
                return;

            if (leftValue.Type != Type.Int || rightValue.Type != Type.Int)
            {
                Error($"Both sides of logical AND expression must be of {Type.Int} type.");
                return;
            }

            if (leftValue.IsEqualTo(0) || rightValue.IsEqualTo(0))
                lastExpressionValue = ValuesFactory.CreateValue(0);
            else
                lastExpressionValue = ValuesFactory.CreateValue(1);
        }
        public void Visit(EqualityOperator equalityOperator)
        {
            if (!EvaluateBinaryOperator(equalityOperator, out IValueReference leftValue, out IValueReference rightValue))
                return;

            if (leftValue.Type != Type.Int || rightValue.Type != Type.Int)
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
            lastExpressionValue = ValuesFactory.CreateValue(value);
        }
        public void Visit(RelationOperator relationOperator)
        {
            if (!EvaluateBinaryOperator(relationOperator, out IValueReference leftValue, out IValueReference rightValue))
                return;

            if (leftValue.Type != Type.Int || rightValue.Type != Type.Int)
            {
                Error($"Both sides of relation expression must be of {Type.Int} type.");
                return;
            }

            int left = (leftValue as IntValueReference).Value;
            int right = (rightValue as IntValueReference).Value;
            int value = relationOperator.OperatorType switch
            {
                RelationOperatorType.LessEqual => left <= right ? 1 : 0,
                RelationOperatorType.GreaterEqual => left >= right ? 1 : 0,
                RelationOperatorType.LessThan => left < right ? 1 : 0,
                RelationOperatorType.GreaterThan => left > right ? 1 : 0,
                _ => throw new ArgumentException("Invalid relation operator type.", nameof(relationOperator))
            };
            lastExpressionValue = ValuesFactory.CreateValue(value);
        }

        public void VisitAdditiveOperator_OnIntegers(IValueReference leftValue, IValueReference rightValue, AdditiveOperatorType operatorType)
        {
            if (leftValue.Type != Type.Int || rightValue.Type != Type.Int)
            {
                Error($"Both sides of additive expression must be of {Type.Int.ToString().ToLower()} or {Type.String.ToString().ToLower()} type.");
                return;
            }

            int left = (leftValue as IntValueReference).Value;
            int right = (rightValue as IntValueReference).Value;
            int value;
            try
            {
                value = operatorType switch
                {
                    AdditiveOperatorType.Add => checked(left + right),
                    AdditiveOperatorType.Subtract => checked(left - right),
                    _ => throw new ArgumentException("Invalid additive operator type.", nameof(operatorType))
                };
            }
            catch (OverflowException)
            {
                Error($"int overflow.");
                return;
            }

            lastExpressionValue = ValuesFactory.CreateValue(value);
        }
        public static bool TryStringConcatenation(IValueReference leftValue, IValueReference rightValue, out StringValueReference resultValue)
        {
            if (leftValue.Type == Type.String || rightValue.Type == Type.String)
            {
                resultValue = ValuesFactory.CreateValue($"{leftValue.Value}{rightValue.Value}");
                return true;
            }
            resultValue = null;
            return false;
        }
        public void Visit(AdditiveOperator additiveOperator)
        {
            if (!EvaluateBinaryOperator(additiveOperator, out IValueReference leftValue, out IValueReference rightValue))
                return;

            if (additiveOperator.OperatorType == AdditiveOperatorType.Add &&
                TryStringConcatenation(leftValue, rightValue, out StringValueReference concatenatedValue))
            {
                lastExpressionValue = concatenatedValue;
                return;
            }

            VisitAdditiveOperator_OnIntegers(leftValue, rightValue, additiveOperator.OperatorType);
        }
        public void Visit(MultiplicativeOperator multiplicativeOperator)
        {
            if (!EvaluateBinaryOperator(multiplicativeOperator, out IValueReference leftValue, out IValueReference rightValue))
                return;

            if (leftValue is null || rightValue is null ||
                leftValue.Type != Type.Int || rightValue.Type != Type.Int)
            {
                Error($"Both sides of multiplicative expression must be of {Type.Int} type.");
                return;
            }

            int left = (leftValue as IntValueReference).Value;
            int right = (rightValue as IntValueReference).Value;
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

            lastExpressionValue = ValuesFactory.CreateValue(value);
        }
        public void Visit(UnaryOperator unaryOperator)
        {
            unaryOperator.Expression.Accept(this);
            if (error)
                return;

            ConsumeLastExpressionValue(out IValueReference value);

            if (value.Type != Type.Int)
            {
                Error($"Unary minus expression must be of {Type.Int} type.");
                return;
            }
            int intVal = (value as IntValueReference).Value;
            int val = unaryOperator.OperatorType switch
            {
                UnaryOperatorType.Uminus => -intVal,
                UnaryOperatorType.LogicalNegation => intVal == 0 ? 1 : 0,
                _ => throw new ArgumentException("Invalid unary operator type.", nameof(unaryOperator))
            };

            lastExpressionValue = ValuesFactory.CreateValue(val);
        }
        #endregion
        #endregion
    }
}
