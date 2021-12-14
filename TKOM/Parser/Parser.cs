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
            Move();

            IList<FunctionDefinition> functions = new List<FunctionDefinition>();
            while (TryParseFunctionDefinition(out FunctionDefinition function))
            {
                functions.Add(function);
            }
            if (functions.Count == 0)
            {
                errorHandler.HandleError("Program should have an entry point.");        // TODO: move error handling somewhere else
                return false;
            }
            if (scanner.Current != Token.EOF)
                return false;

            program = new Program(functions);
            return true;
        }
        private bool TryParseToken(Token expectedToken)
        {
            if (scanner.Current != expectedToken)
            {
                //errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                return false;
            }
            Move();
            return true;
        }
        private bool TryParseToken(Token expectedToken, out string stringValue)
        {
            if (scanner.Current != expectedToken)
            {
                //errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                stringValue = null;
                return false;
            }
            stringValue = scanner.StringValue;
            Move();
            return true;
        }
        private bool TryParseToken(Token expectedToken, out int? intValue)
        {
            if (scanner.Current != expectedToken)
            {
                //errorHandler.HandleError($"Syntax error, {expectedToken} expected.");
                intValue = null;
                return false;
            }
            intValue = scanner.IntValue;
            Move();
            return true;
        }
        private bool TokenIs(Token expectedToken)
        {
            return scanner.Current == expectedToken;
        }
        //private bool MoveAndCheckFor(Token expectedToken)
        //{
        //    if (!scanner.MoveNext() || scanner.Current != expectedToken)
        //        return false;
        //    return true;
        //}
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
            if (TokenIs(Token.EOF))
                return false;
            Type? type;
            if (TryParseToken(Token.Void))
                type = Type.Void;
            else if (!TryParseTypeToken(out type))
            {
                errorHandler.HandleError("Syntax error, function definition expected.");
                return false;
            }

            if (!TryParseToken(Token.Identifier, out string functionName) ||
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
            if (!TryParseToken(Token.CurlyBracketOpen))
                return false;

            var statements = new List<IStatement>();
            while (!TryParseToken(Token.CurlyBracketClose))
            {
                if (TryParseStatement(out IStatement statement))
                    statements.Add(statement);
                else
                    return false;
            }

            block = new Block(statements);
            return true;
        }
        // statement           : simple_statement ";" | block_statement
        private bool TryParseStatement(out IStatement statement)
        {
            if (TryParseSimpleStatement(out statement) ||
                TryParseBlockStatement(out statement))
                return true;
            if (TryParseBlock(out Block block))
            {
                statement = block;
                return true;
            }
            return false;
        }
        // statement           : block_statement
        private bool TryParseBlockStatement(out IStatement statement)
        {
            statement = null;
            if (TryParseIfStatement(out If ifStatement))                // block_statement     : if
                statement = ifStatement;
            else if (TryParseWhileStatement(out While whileStatement))  // block_statement     : while
                statement = whileStatement;
            else if (TryParseTryCatchFinallyStatement(out TryCatchFinally tcfStatement))
                statement = tcfStatement;
            else if (TryParseBlock(out Block block))
                statement = block;
            else
                return false;
            // block_statement     : try_catch_finally
            return true;
        }
        // if                  : "if" "(" expression ")" statement [ "else" statement ]
        private bool TryParseIfStatement(out If ifStatement)
        {
            ifStatement = null;
            if (!TryParseToken(Token.If) ||
                !TryParseToken(Token.RoundBracketOpen) ||
                !TryParseExpression(out IExpression condition) ||
                !TryParseToken(Token.RoundBracketClose) ||
                !TryParseStatement(out IStatement stmt))
                return false;
            
            if (TryParseToken(Token.Else))
            {
                if (TryParseStatement(out IStatement elseStmt))
                {
                    ifStatement = new If(condition, stmt, elseStmt);
                    return true;
                }
                return false;
            }

            ifStatement = new If(condition, stmt);
            return true;
        }
        // while               : "while" "(" expression ")" statement
        private bool TryParseWhileStatement(out While whileStatement)
        {
            whileStatement = null;
            if (!TryParseToken(Token.While) ||
                !TryParseToken(Token.RoundBracketOpen) ||
                !TryParseExpression(out IExpression expression) ||
                !TryParseToken(Token.RoundBracketClose) ||
                !TryParseStatement(out IStatement statement))
                return false;
            whileStatement = new While(expression, statement);
            return true;
        }

        // try_catch_finally   : "try" statement catch { catch } [ "finally" statement]
        private bool TryParseTryCatchFinallyStatement(out TryCatchFinally tcfStatement)
        {
            tcfStatement = null;
            if (!TryParseToken(Token.Try) ||
                !TryParseStatement(out IStatement tryStatement) ||
                !TryParseCatch(out Catch @catch))
                return false;
            IList<Catch> catches = new List<Catch>();
            catches.Add(@catch);

            while (TryParseCatch(out Catch catchBlock))
                catches.Add(catchBlock);
            if (TryParseToken(Token.Finally))
            {
                if (!TryParseStatement(out IStatement finallyStatement))
                    return false;
                tcfStatement = new TryCatchFinally(tryStatement, catches, finallyStatement);
                return true;
            }
            tcfStatement = new TryCatchFinally(tryStatement, catches);
            return true;
        }
        // catch               : "catch" "Exception" IDENTIFIER [ "when" expression ] statement
        private bool TryParseCatch(out Catch catchBlock)
        {
            catchBlock = null;
            if (!TryParseToken(Token.Catch) ||
                !TryParseToken(Token.Exception) ||
                !TryParseToken(Token.Identifier, out string variable))
                return false;
            
            if (TryParseToken(Token.When))
            {
                if (!TryParseExpression(out IExpression expression) ||
                    !TryParseStatement(out IStatement stmt))
                    return false;
                catchBlock = new Catch(variable, stmt, expression);
                return true;
            }

            if (!TryParseStatement(out IStatement statement))
                return false;
            
            catchBlock = new Catch(variable, statement);
            return true;
        }

        // statement           : simple_statement ";"
        private bool TryParseSimpleStatement(out IStatement statement)
        {
            statement = null;
            if (!TryParseDeclarationsWithOptionalAssignments(out statement) &&
                !TryParseAssignmentOrFunctionCall(out statement) &&
                !TryParseThrowStatementRest(out statement) &&
                !TryParseReturnStatement(out statement))
                return false;

            // ";"
            if (!TryParseToken(Token.Semicolon))
            {
                statement = null;
                return false;
            }
            return true;
        }
        private bool TryParseReturnStatement(out IStatement statement)
        {
            statement = null;
            if (!TryParseToken(Token.Return))
                return false;

            if (TokenIs(Token.Semicolon))
            {
                statement = new Return();
                return true;
            }
            if (!TryParseExpression(out IExpression expression))
                return false;
            statement = new Return(expression);
            return true;
        }
        // "Exception" "(" expression ")"
        private bool TryParseThrowStatementRest(out IStatement statement)
        {
            statement = null;
            if (!TryParseToken(Token.Throw) ||
                !TryParseToken(Token.Exception) ||
                !TryParseToken(Token.RoundBracketOpen) ||
                !TryParseExpression(out IExpression expression) ||
                !TryParseToken(Token.RoundBracketClose))
                return false;
            statement = new Throw(expression);
            return true;
        }

        private bool TryParseExpression(out IExpression expression)
        {
            expression = null;
            if (TryParseToken(Token.IntConst, out int? value))
            {
                expression = new IntConst(value.Value);
                return true;
            }
            if (TryParseToken(Token.Identifier, out string variable))
            {
                if (!TryParseToken(Token.RoundBracketOpen))
                {
                    expression = new Variable(variable);
                    return true;
                }
                // TODO: tryParse funcall
                return false;
            }
            return false;
        }

        // simple_statement    : assignment | function_call
        private bool TryParseAssignmentOrFunctionCall(out IStatement statement)
        {
            statement = null;
            if (!TryParseToken(Token.Identifier, out string identifier))
                return false;

            // lvalue "=" expression
            if (TryParseToken(Token.Equals))
            {
                if (!TryParseToken(Token.IntConst, out int? intVal))
                    return false;
                statement = new Assignment(identifier, new IntConst(intVal.Value));
                return true;
            }

            // function_call       : IDENTIFIER "(" ...
            if (TryParseToken(Token.RoundBracketOpen))
            {
                if (!TryParseFunctionArgumentsRest(out IList<IExpression> arguments))
                    return false;
                statement = new FunctionCall(identifier, arguments);
                return true;
            }
            return false;
        }

        // [ expression { "," expression } ] ")"
        private bool TryParseFunctionArgumentsRest(out IList<IExpression> expressions)
        {
            expressions = new List<IExpression>();
            if (TryParseToken(Token.RoundBracketClose))
                return true;
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
        private bool TryParseDeclarationsWithOptionalAssignments(out IStatement statement)  // TODO: add optional assignment
        {
            if (!TryParseTypeToken(out Type? type) ||
                !TryParseToken(Token.Identifier, out string identifier))
            {
                statement = null;
                return false;
            }
            statement = new Declaration(type.Value, identifier);
            return true;
        }

        // param_list           : "(" [ type IDENTIFIER { "," type IDENTIFIER } ] ")"
        private bool TryParseParameters(out IList<Parameter> parameters)
        {
            parameters = null;
            if (!TryParseToken(Token.RoundBracketOpen))
                return false;

            parameters = new List<Parameter>();
            if (TryParseTypeToken(out Type? t))
            {
                if (!TryParseToken(Token.Identifier, out string param))
                {
                    parameters = null;
                    return false;
                }
                parameters.Add(new Parameter(t.Value, param));
            }
                
            while (TryParseToken(Token.Comma))
            {
                if (!TryParseTypeToken(out Type? type) ||
                    !TryParseToken(Token.Identifier, out string paramName))
                {
                    parameters = null;
                    return false;
                }
                parameters.Add(new Parameter(type.Value, paramName));
            }

            if (!TryParseToken(Token.RoundBracketClose))
            {
                parameters = null;
                return false;
            }
            return true;
        }

        private bool TryParseTypeToken(out Type? type)
        {
            bool intParsed = TryParseToken(Token.Int);
            type = intParsed ? Type.IntType : null;
            return intParsed;
        }
        private bool IsTypeToken(Token token, out Type? type)
        {
            if (token == Token.Int)
            {
                type = Type.IntType;
                return true;
            }
            type = null;
            return false;
        }
    }
}
