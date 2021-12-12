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
            //scanner.MoveNext();
        }

        public bool TryParse(out Program program)
        {
            program = null;
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
        private bool MoveAndAssertFor(Token expectedToken)
        {
            if (!MoveAndCheckFor(expectedToken))
            {
                errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                return false;
            }
            return true;
        }
        private bool MoveAndAssertFor(Token expectedToken, out string stringValue)
        {
            if (!MoveAndCheckFor(expectedToken, out stringValue))
            {
                errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                return false;
            }
            return true;
        }
        private bool MoveAndAssertFor(Token expectedToken, out int? intValue)
        {
            if (!MoveAndCheckFor(expectedToken, out intValue))
            {
                errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                return false;
            }
            return true;
        }
        private bool MoveAndCheckFor(Token expectedToken)
        {
            if (!scanner.MoveNext() || scanner.Current != expectedToken)
                return false;
            return true;
        }
        private bool MoveAndCheckFor(Token expectedToken, out string stringValue)
        {
            if (!scanner.MoveNext() || scanner.Current != expectedToken)
            {
                stringValue = null;
                return false;
            }
            stringValue = scanner.StringValue;
            return true;
        }
        private bool MoveAndCheckFor(Token expectedToken, out int? intValue)
        {
            if (!scanner.MoveNext() || scanner.Current != expectedToken)
            {
                intValue = null;
                return false;
            }
            intValue = scanner.IntValue;
            return true;
        }
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
            scanner.MoveNext();
            if (scanner.Current == Token.EOF)
                return false;
            if (!TryCastTokenToType(scanner.Current, out Type? type))
            {
                errorHandler.HandleError("Syntax error, function definition expected.");
                return false;
            }
            if (!MoveAndAssertFor(Token.Identifier, out string functionName) ||
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
            if (!MoveAndAssertFor(Token.CurlyBracketOpen))
                return false;

            if (!Move())
                return false;
            var statements = new List<IStatement>();
            if (TryCastTokenToType(scanner.Current, out Type? _))
            {
                if (TryParseSimpleStatement(out IStatement statement))
                    statements.Add(statement);
                else 
                    return false;
            }
            else
                switch (scanner.Current)
                {
                    case Token.CurlyBracketClose:
                        block = new Block(statements);
                        return true;
                    default:
                        return false;
                }

            if (!MoveAndAssertFor(Token.CurlyBracketClose))
                return false;

            block = new Block(statements);
            return true;
        }

        private bool TryParseSimpleStatement(out IStatement statement)
        {
            statement = null;
            if (TryCastTokenToType(scanner.Current, out Type? _))
            {
                if (TryParseDeclaration(out Declaration declaration))
                    statement = declaration;
                else
                    return false;
            }
            if (!MoveAndAssertFor(Token.Semicolon))
                return false;
            return true;
        }

        // declaration         : type declOptAssign { "," decl_opt_assign }
        private bool TryParseDeclaration(out Declaration declaration)
        {
            if (!TryCastTokenToType(scanner.Current, out Type? type) ||
                !MoveAndAssertFor(Token.Identifier, out string identifier))
            {
                declaration = null;
                return false;
            }
            declaration = new Declaration(type.Value, identifier);
            return true;
        }

        private bool TryParseNotVoidType(out Type? type)
        {
            if (!MoveAndAssertFor(Token.Int))
            {
                type = null;
                return false;
            }
            type = Type.IntType;
            return true;
        }

        // param_list           : "(" [ type IDENTIFIER { "," type IDENTIFIER } ] ")"
        private bool TryParseParameters(out IList<Parameter> parameters)
        {
            parameters = null;
            if (!MoveAndAssertFor(Token.RoundBracketOpen))
                return false;

            parameters = new List<Parameter>();
            do
            {
                if (!Move())
                    return false;
                if (scanner.Current == Token.Int)
                {
                    if (!MoveAndAssertFor(Token.Identifier, out string paramName))
                    {
                        parameters = null;
                        return false;
                    }
                    parameters.Add(new Parameter(Type.IntType, paramName));
                }
                else if (scanner.Current == Token.RoundBracketClose)
                    return true;
                else
                {
                    errorHandler.HandleError(new LexLocation(scanner.Position, scanner.Position), $"Unexpected token {scanner.Current}.");
                    return false;
                }

                if (!MoveAndCheckFor(Token.Comma))
                {
                    if (scanner.Current != Token.RoundBracketClose)
                    {
                        parameters = null;
                        return false;
                    }
                    return true;
                }

            } while (true);
        }

        private bool TryCastTokenToType(Token token, out Type? type)
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
