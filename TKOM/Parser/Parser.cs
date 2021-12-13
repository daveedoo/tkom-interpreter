using System.Collections.Generic;
using TKOM.ErrorHandler;
using TKOM.Node;
using TKOM.Scanner;

namespace TKOM.Parser
{
    public class Parser : IParser
    {
        private IScanner scanner { get; }
        private IErrorHandler errorHandler { get; }

        public Parser(IScanner scanner, IErrorHandler errorHandler)
        {
            this.scanner = scanner;
            this.errorHandler = errorHandler;
        }

        public bool TryParse(out Program program)
        {
            program = null;
            if (!Move())
                return false;
            IList<FunctionDefinition> functions = new List<FunctionDefinition>();
            while (TryParseFunctionDefinition(out FunctionDefinition function))
            {
                functions.Add(function);
            }
            if (functions.Count == 0)
            {
                errorHandler.HandleError("Program should have an entry point.");
                return false;
            }
            if (scanner.Current != Token.EOF)
                return false;

            program = new Program(functions);
            return true;
        }
        private bool AssertForAndMove(Token expectedToken)
        {
            if (scanner.Current != expectedToken)
            {
                errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                return false;
            }
            Move();
            return true;
        }
        private bool AssertForAndMove(Token expectedToken, out string stringValue)
        {
            if (scanner.Current != expectedToken)
            {
                errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                stringValue = null;
                return false;
            }
            stringValue = scanner.StringValue;
            Move();
            return true;
        }
        private bool AssertForAndMove(Token expectedToken, out int? intValue)
        {
            if (scanner.Current != expectedToken)
            {
                errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                intValue = null;
                return false;
            }
            intValue = scanner.IntValue;
            Move();
            return true;
        }
        private bool MoveAndCheckFor(Token expectedToken)
        {
            if (!scanner.MoveNext() || scanner.Current != expectedToken)
                return false;
            return true;
        }
        //private bool MoveAndCheckFor(Token expectedToken, out string stringValue)
        //{
        //    if (!scanner.MoveNext() || scanner.Current != expectedToken)
        //    {
        //        stringValue = null;
        //        return false;
        //    }
        //    stringValue = scanner.StringValue;
        //    return true;
        //}
        //private bool MoveAndCheckFor(Token expectedToken, out int? intValue)
        //{
        //    if (!scanner.MoveNext() || scanner.Current != expectedToken)
        //    {
        //        intValue = null;
        //        return false;
        //    }
        //    intValue = scanner.IntValue;
        //    return true;
        //}
        private bool Move()
        {
            if (!scanner.MoveNext())
            {
                errorHandler.HandleError(new LexLocation(scanner.Position, scanner.Position), "Unexpected EOF.");   // TODO: add HandleError(Position, string)
                return false;
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="funDef">Function definition or null in case of any error.</param>
        /// <returns>Value indicating if read <paramref name="funDef"/> is valid.</returns>
        // function             : ( "void" | type ) IDENTIFIER "(" [param_list] ")" block
        private bool TryParseFunctionDefinition(out FunctionDefinition funDef)
        {
            funDef = null;
            //scanner.MoveNext();
            if (scanner.Current == Token.EOF)
                return false;
            if (!IsTokenAType(scanner.Current, out Type? type))
            {
                errorHandler.HandleError("Syntax error, function definition expected.");
                return false;
            }
            Move();

            if (!AssertForAndMove(Token.Identifier, out string functionName) ||
                !TryParseParameters(out IList<Parameter> parameters) ||
                !TryParseBlock(out Block block))
                return false;

            funDef = new FunctionDefinition(type.Value, functionName, parameters, block);
            return true;
        }

        // block                : "{" { statement } "}"
        private bool TryParseBlock(out Block block)
        {
            block = null;
            if (scanner.Current != Token.CurlyBracketOpen)
                return false;
            Move();

            var statements = new List<IStatement>();
            while (scanner.Current != Token.CurlyBracketClose)
            {
                bool doSimpleStmt = IsTokenAType(scanner.Current, out Type? _) ||
                    scanner.Current switch
                    {
                        Token.Identifier
                        or Token.Return => true,
                        _ => false
                    };

                if (doSimpleStmt)
                {
                    if (!TryParseSimpleStatement(out IList<IStatement> statementsList))
                        return false;
                    statements.AddRange(statementsList);
                }
            }
            Move();

            block = new Block(statements);
            return true;
        }
        // statement           : simple_statement ";"
        private bool TryParseSimpleStatement(out IList<IStatement> statementsList)
        {
            statementsList = null;
            if (IsTokenAType(scanner.Current, out Type? type))
            {
                if (!TryParseDeclarationsWithOptionalAssignments(out IList<IStatement> statements))
                    return false;
                statementsList = statements;
            }
            else if (scanner.Current == Token.Identifier)
            {
                if (!TryParseAssignmentOrFunctionCall(out IStatement statement))
                    return false;
                statementsList = new List<IStatement> { statement };
            }
            else if (scanner.Current == Token.Return)
            {
                Move();
                if (!TryParseExpression(out IExpression expression))
                    return false;
                statementsList = new List<IStatement> { new Return(expression) };
            }

            if (scanner.Current != Token.Semicolon)
            {
                statementsList = null;
                return false;
            }
            Move();
            return true;
        }

        private bool TryParseExpression(out IExpression expression)
        {
            expression = null;
            if (scanner.Current == Token.IntConst)
            {
                expression = new IntConst(scanner.IntValue.Value);
                Move();
                return true;
            }
            else if (scanner.Current == Token.Identifier)
            {
                string variableName = scanner.StringValue;
                Move();
                if (scanner.Current != Token.RoundBracketOpen)
                {
                    expression = new Variable(variableName);
                    return true;
                }
                // TODO: tryParse funcall
                return false;
            }
            return false;
        }

        private bool TryParseAssignmentOrFunctionCall(out IStatement statement)
        {
            statement = null;
            if (!AssertForAndMove(Token.Identifier, out string identifier))
                return false;
            if (scanner.Current == Token.Equals)
            {
                Move();
                if (!AssertForAndMove(Token.IntConst, out int? intVal))
                    return false;
                statement = new Assignment(identifier, new IntConst(intVal.Value));
                return true;
            }
            else if (scanner.Current == Token.RoundBracketOpen)
            {
                if (!TryParseFunctionArguments(out IList<IExpression> arguments))
                    return false;
                statement = new FunctionCall(identifier, arguments);
                return true;
            }
            return false;
        }

        private bool TryParseFunctionArguments(out IList<IExpression> expressions)
        {
            expressions = null;
            if (scanner.Current != Token.RoundBracketOpen)
                return false;
            Move();

            expressions = new List<IExpression>();
            if (scanner.Current == Token.RoundBracketClose)
            {
                Move();
                return true;
            }
            do
            {
                if (!TryParseExpression(out IExpression expression))
                    return false;
                expressions.Add(expression);

                switch (scanner.Current)
                {
                    case Token.Comma:
                        Move();
                        break;
                    case Token.RoundBracketClose:
                        Move();
                        return true;
                    default:
                        return false;
                }
            } while (true);
        }

        // declaration         : type declOptAssign { "," decl_opt_assign }
        private bool TryParseDeclarationsWithOptionalAssignments(out IList<IStatement> statements)
        {
            if (!IsTokenAType(scanner.Current, out Type? type))
            {
                statements = null;
                return false;
            }
            Move();
            if (!AssertForAndMove(Token.Identifier, out string identifier))
            {
                statements = null;
                return false;
            }
            statements = new List<IStatement> { new Declaration(type.Value, identifier) };
            return true;
        }

        // param_list           : "(" [ type IDENTIFIER { "," type IDENTIFIER } ] ")"
        private bool TryParseParameters(out IList<Parameter> parameters)
        {
            parameters = null;
            if (scanner.Current != Token.RoundBracketOpen)
                return false;

            parameters = new List<Parameter>();
            do
            {
                if (!Move())
                    return false;
                if (scanner.Current == Token.Int)
                {
                    Move();
                    if (!AssertForAndMove(Token.Identifier, out string paramName))
                    {
                        parameters = null;
                        return false;
                    }
                    parameters.Add(new Parameter(Type.IntType, paramName));
                }
                else if (scanner.Current == Token.RoundBracketClose)
                {
                    Move();
                    return true;
                }
                else
                {
                    errorHandler.HandleError(new LexLocation(scanner.Position, scanner.Position), $"Unexpected token {scanner.Current}.");
                    return false;
                }

                if (scanner.Current != Token.Comma)
                {
                    if (scanner.Current == Token.RoundBracketClose)
                    {
                        Move();
                        return true;
                    }
                    parameters = null;
                    return false;
                }

            } while (true);
        }

        private bool IsTokenAType(Token token, out Type? type)
        {
            type = token switch
            {
                Token.Void => Type.Void,
                Token.Int => Type.IntType,
                _ => null
            };
            return type is null ? false : true;
        }
    }
}
