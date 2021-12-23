using System.Collections.Generic;
using System.Linq;
using TKOM.ErrorHandler;
using TKOM.Node;
using TKOM.Scanner;

namespace TKOM.Parser
{
    public class Parser : IParser
    {
        private IScanner scanner { get; }
        private IErrorHandler errorHandler { get; }
        private bool parsed;

        public Parser(IScanner scanner, IErrorHandler errorHandler)
        {
            this.scanner = scanner;
            this.errorHandler = errorHandler;
            parsed = false;
        }



        public bool TryParse(out Program program)                                           // program             : { function } EOF
        {
            program = null;
            if (parsed)
                return false;

            Move();

            IList<FunctionDefinition> functions = new List<FunctionDefinition>();
            while (scanner.Current != Token.EOF)
            {
                if (!TryParseFunctionDefinition(out FunctionDefinition function))
                {
                    Position errStartPos = scanner.Position;
                    while (scanner.Current != Token.Int && scanner.Current != Token.Void && scanner.Current != Token.EOF)   // functionDef's firsts (+ EOF)
                        Move();
                    errorHandler.Error(new LexLocation(errStartPos, scanner.Position), "Expected proper function definition.");
                }
                functions.Add(function);
            }

            program = new Program(functions);
            parsed = true;
            return true;
        }

        #region helper methods
        private bool TryParseToken(Token expectedToken, bool errorMsg = true)
        {
            if (scanner.Current != expectedToken)
            {
                if (errorMsg)
                    errorHandler.Error(scanner.Position, $"Syntax error, {expectedToken} expected.");
                return false;
            }
            Move();
            return true;
        }
        private bool TryParseToken(Token expectedToken, out string stringValue, bool errorMsg = true)
        {
            if (scanner.Current != expectedToken)
            {
                if (errorMsg)
                    errorHandler.Error(scanner.Position, $"Syntax error, {expectedToken} expected.");
                stringValue = null;
                return false;
            }
            stringValue = scanner.StringValue;
            Move();
            return true;
        }
        private bool TryParseToken(Token expectedToken, out int? intValue, bool errorMsg = true)
        {
            if (scanner.Current != expectedToken)
            {
                if (errorMsg)
                    errorHandler.Error(scanner.Position, $"Syntax error, {expectedToken} expected.");
                intValue = null;
                return false;
            }
            intValue = scanner.IntValue;
            Move();
            return true;
        }
        private bool Move()
        {
            if (!scanner.MoveNext())
            {
                //errorHandler.HandleError(new LexLocation(scanner.Position, scanner.Position), "Unexpected EOF.");   // TODO: add HandleError(Position, string)
                return false;
            }
            return true;
        }
        private void ParseUntilTokens(bool errorMsg = true, params Token[] expected)
        {
            if (expected.Length == 0)
                return;

            Position errStart = scanner.Position;
            if (expected.Contains(scanner.Current))
                return;
            
            do
            {
                if (!Move())
                    break;
            }
            while (!expected.Contains(scanner.Current));
            
            if (errorMsg)
            {
                LexLocation errors = new LexLocation(errStart, scanner.Position);
                errorHandler.Error(errors, $"Syntax error, extra tokens.");
            }
        }
        private bool TryParseTypeToken(out Type? type, bool errorMsg = true)                // type                : "int"
        {
            bool intParsed = TryParseToken(Token.Int, errorMsg);
            type = intParsed ? Type.IntType : null;
            return intParsed;
        }

        #endregion // helper methods

        private bool TryParseFunctionDefinition(out FunctionDefinition funDef)              // function            : ( "void" | type ) IDENTIFIER "(" [ type IDENTIFIER { "," type IDENTIFIER } ] ")" block
        {
            funDef = null;
            if (!TryParseFunctionReturnType(out Type? type, false))
                return false;
            TryParseToken(Token.Identifier, out string functionName);

            TryParseToken(Token.RoundBracketOpen);
            List<Parameter> parameters = new List<Parameter>();
            if (TryParseTypeToken(out Type? type1, false))
            {
                TryParseToken(Token.Identifier, out string param);
                parameters.Add(new Parameter(type1.Value, param));
                while (TryParseToken(Token.Comma, false))
                {
                    TryParseTypeToken(out Type? type2);
                    TryParseToken(Token.Identifier, out string paramName);
                    Parameter p = type2.HasValue ? new Parameter(type2.Value, paramName) : null;
                    parameters.Add(p);
                }
            }
            TryParseToken(Token.RoundBracketClose);

            if (!TryParseBlock(out Block block))
                errorHandler.Error("Syntax error, function body expected.");

            funDef = new FunctionDefinition(type.Value, functionName, parameters, block);
            return true;
        }
        private bool TryParseFunctionReturnType(out Type? type, bool errorMsg = true)       // ( "void" | type )
        {
            if (TryParseTypeToken(out type, false))
                return true;
            if (TryParseToken(Token.Void, false))
            {
                type = Type.Void;
                return true;
            }
            if (errorMsg)
                errorHandler.Error("Syntax error, return type expected.");
            return false;

        }

        private bool TryParseBlock(out Block block)                                         // block                : "{" { statement } "}"
        {
            block = null;
            if (!TryParseToken(Token.CurlyBracketOpen, false))
                return false;

            var statements = new List<IStatement>();
            while (TryParseStatement(out IStatement statement))
                statements.Add(statement);
            ParseUntilTokens(true, Token.CurlyBracketClose);
            TryParseToken(Token.CurlyBracketClose);

            block = new Block(statements);
            return true;
        }
        private bool TryParseStatement(out IStatement statement)                            // statement           : simple_statement ";" | block_statement
        {
            return TryParseSimpleStatement(out statement) ||
                    TryParseBlockStatement(out statement);
        }
        private bool TryParseSimpleStatement(out IStatement statement)                      // statement           : simple_statement ";"
        {
            if (TryParseDeclarationsWithOptionalAssignments(out Declaration declaration))   // simple_statement    : declaration
                statement = declaration;
            else if (TryParseReturnStatement(out Return returnStatement))                   //                     | return
                statement = returnStatement;
            else if (TryParseThrowStatement(out Throw throwStatement))                      //                     | throw
                statement = throwStatement;
            else if (!TryParse_Assignment_FunctionCall(out statement))                      //                     | assignment | function_call
                return false;

            ParseUntilTokens(true, Token.Semicolon, Token.CurlyBracketClose);
            TryParseToken(Token.Semicolon);
            return true;
        }
        private bool TryParseBlockStatement(out IStatement statement)                       // statement           : block_statement
        {
            statement = null;
            if (TryParseIfStatement(out If ifStatement))                                    // block_statement     : if
                statement = ifStatement;
            else if (TryParseWhileStatement(out While whileStatement))                      // block_statement     : while
                statement = whileStatement;
            else if (TryParseTryCatchFinallyStatement(out TryCatchFinally tcfStatement))    // block_statement     : try_catch_finally
                statement = tcfStatement;
            else if (TryParseBlock(out Block block))
                statement = block;
            else
                return false;
            return true;
        }
        
        #region simple statements
        // TODO: add optional assignment
        private bool TryParseDeclarationsWithOptionalAssignments(out Declaration statement) // declaration         : type declOptAssign { "," decl_opt_assign }
        {
            statement = null;
            if (!TryParseTypeToken(out Type? type, false))
                return false;
            TryParseToken(Token.Identifier, out string identifier);
            
            statement = new Declaration(type.Value, identifier);
            return true;
        }
        private bool TryParseReturnStatement(out Return statement)                          // return              : "return" [ expression ]
        {
            statement = null;
            if (!TryParseToken(Token.Return, false))
                return false;

            if (TryParseExpression(out IExpression expression))
                statement = new Return(expression);
            else
                statement = new Return();
            return true;
        }
        private bool TryParse_Assignment_FunctionCall(out IStatement statement)             // simple_statement    : assignment | function_call
        {
            statement = null;
            if (!TryParseToken(Token.Identifier, out string identifier, false))
                return false;

            if (TryParseAssignmentRest(identifier, out Assignment assignment))              //  IDENTIFIER "=" expression
                statement = assignment;
            else if (TryParseFunctionArguments(out IList<IExpression> expression))          //  "(" ... ")"
                statement = new FunctionCall(identifier, expression);
            else
                return false;
            return true;
        }
        private bool TryParseAssignmentRest(string identifier, out Assignment assignment)   // assignment           : IDENTIFIER "=" expression
        {
            assignment = null;
            if (!TryParseToken(Token.Equals, false))
                return false;
            if (!TryParseExpression(out IExpression expression))
                errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
            assignment = new Assignment(identifier, expression);
            return true;
        }
        #endregion

        #region block statements
        private bool TryParseIfStatement(out If ifStatement)                                // if                  : "if" "(" expression ")" statement [ "else" statement ]
        {
            ifStatement = null;
            if (!TryParseToken(Token.If, false))
                return false;
            TryParseToken(Token.RoundBracketOpen);
            TryParseExpression(out IExpression condition);
            TryParseToken(Token.RoundBracketClose);
            TryParseStatement(out IStatement stmt);

            if (TryParseToken(Token.Else, false))
            {
                TryParseStatement(out IStatement elseStmt);
                ifStatement = new If(condition, stmt, elseStmt);
                return true;
            }

            ifStatement = new If(condition, stmt);
            return true;
        }
        private bool TryParseWhileStatement(out While whileStatement)                       // while               : "while" "(" expression ")" statement
        {
            whileStatement = null;
            if (!TryParseToken(Token.While, false))
                return false;
            TryParseToken(Token.RoundBracketOpen);
            TryParseExpression(out IExpression expression);
            TryParseToken(Token.RoundBracketClose);
            TryParseStatement(out IStatement statement);

            whileStatement = new While(expression, statement);
            return true;
        }
        
        private bool TryParseThrowStatement(out Throw statement)                            // throw               : "throw" "Exception" "(" expression ")"
        {
            statement = null;
            if (!TryParseToken(Token.Throw, false))
                return false;
            TryParseToken(Token.Exception);
            TryParseToken(Token.RoundBracketOpen);
            TryParseExpression(out IExpression expression);
            TryParseToken(Token.RoundBracketClose);

            statement = new Throw(expression);
            return true;
        }
        private bool TryParseTryCatchFinallyStatement(out TryCatchFinally tcfStatement)     // try_catch_finally   : "try" statement catch { catch } [ "finally" statement]
        {
            tcfStatement = null;
            if (!TryParseToken(Token.Try, false))
                return false;
            TryParseStatement(out IStatement tryStatement);
            TryParseCatch(out Catch @catch);

            IList<Catch> catches = new List<Catch>();
            catches.Add(@catch);
            while (TryParseCatch(out Catch catchBlock))
                catches.Add(catchBlock);

            if (TryParseToken(Token.Finally, false))
            {
                TryParseStatement(out IStatement finallyStatement);
                tcfStatement = new TryCatchFinally(tryStatement, catches, finallyStatement);
                return true;
            }
            tcfStatement = new TryCatchFinally(tryStatement, catches);
            return true;
        }
        private bool TryParseCatch(out Catch catchBlock)                                    // catch               : "catch" "Exception" IDENTIFIER [ "when" expression ] statement
        {
            catchBlock = null;
            if (!TryParseToken(Token.Catch, false))
                return false;
            TryParseToken(Token.Exception);
            TryParseToken(Token.Identifier, out string variable);

            IExpression expression = null;
            if (TryParseToken(Token.When, false))
                TryParseExpression(out expression);

            TryParseStatement(out IStatement statement);
            catchBlock = new Catch(variable, statement, expression);
            return true;
        }
        #endregion

        #region expressions
        private bool TryParseExpression(out IExpression expression)                         // expression          : logical_or
        {
            return TryParseLogicalOr(out expression);
        }        
        private bool TryParseLogicalOr(out IExpression expression)                          // logical_or          : logical_and { "||" logical_and }
        {
            expression = null;
            if (!TryParseLogicalAnd(out IExpression left))
                return false;
            expression = left;
            while (TryParseToken(Token.Or, false))
            {
                if (!TryParseLogicalAnd(out IExpression right))
                    errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                expression = new LogicalOr(expression, right);
            }
            return true;
        }
        private bool TryParseLogicalAnd(out IExpression expression)                         // logical_and         : in_equality { "&&" in_equality }
        {
            expression = null;
            if (!TryParseEqualityComparers(out IExpression left))
                return false;
            expression = left;
            while (TryParseToken(Token.And, false))
            {
                if (!TryParseEqualityComparers(out IExpression right))
                    errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                expression = new LogicalAnd(expression, right);
            }
            return true;
        }
        private bool TryParseEqualityComparers(out IExpression expression)                  // in_equality         : relation [ ( "==" | "!=" ) relation ]
        {
            expression = null;
            if (!TryParseRelation(out IExpression left))
                return false;
            expression = left;
            if (TryParseEqualityComparerOperator(out EqualityComparerType? comparerType))
            {
                if (!TryParseRelation(out IExpression right))
                    errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                expression = new EqualityComparer(expression, comparerType.Value, right);
            }
            return true;
        }
        private bool TryParseRelation(out IExpression expression)                           // relation            : additive [ ( "<=" | ">=" | "<" | ">" ) additive ]
        {
            expression = null;
            if (!TryParseAdditive(out IExpression left))
                return false;
            expression = left;
            
            if (TryParseRelationOperator(out RelationType? relation))
            {
                if(!TryParseAdditive(out IExpression right))
                    errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                expression = new RelationOperator(left, relation.Value, right);
            }
            return true;
        }
        private bool TryParseAdditive(out IExpression expression)                           // additive            : multiplicative { ( "+" | "-" ) multiplicative }
        {
            if (!TryParseMultiplicative(out expression))
                return false;
            while (TryParseAdditiveOperator(out AdditiveOperator? additiveOperator))
            {
                if (!TryParseMultiplicative(out IExpression right))
                    errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                expression = new Additive(expression, additiveOperator.Value, right);
            }
            return true;
        }
        private bool TryParseMultiplicative(out IExpression expression)                     // multiplicative      : unar { ( "*" | "/" ) unar }
        {
            if (!TryParseUnary(out expression))
                return false;
            while (TryParseMultiplicativeOperator(out MultiplicativeOperator? multiplicativeOperator))
            {
                if (!TryParseUnary(out IExpression unary))
                    errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                expression = new Multiplicative(expression, multiplicativeOperator.Value, unary);
            }
            return true;
        }
        private bool TryParseUnary(out IExpression unary)                                   // unar                : [ "-" | "!" ] ( ( "(" expression ")" ) | atomic )
        {
            TryParseUnaryOperator(out UnaryOperator? unaryOperator);

            if (TryParseToken(Token.RoundBracketOpen, false))
            {
                TryParseExpression(out unary);
                TryParseToken(Token.RoundBracketClose);
                return true;
            }
            else
            {
                if (!TryParseAtomic(out unary))
                {
                    if (unaryOperator.HasValue)
                        errorHandler.Error(scanner.Position, "Syntax error, const value or identifier expected.");
                    else
                        return false;
                }
            }

            if (unaryOperator.HasValue)
                unary = new Unary(unaryOperator.Value, unary);
            return true;
        }
        private bool TryParseAtomic(out IExpression expression, bool errorMsg = true)       // atomic              : const | IDENTIFIER | function_call | string
        {
            expression = null;
            if (TryParseToken(Token.IntConst, out int? intConst, false))
                expression = new IntConst(intConst.Value);
            else if (TryParseToken(Token.Identifier, out string identifier, false))
            {
                if (TryParseFunctionArguments(out IList<IExpression> expressions))
                    expression = new FunctionCall(identifier, expressions);
                else
                    expression = new Variable(identifier);
            }
            else
                return false;
            return true;
        }

        private bool TryParseFunctionArguments(out IList<IExpression> expressions)          // "(" [ expression { "," expression } ] ")"
        {
            expressions = null;
            if (!TryParseToken(Token.RoundBracketOpen, false))
                return false;

            expressions = new List<IExpression>();
            if (TryParseExpression(out IExpression expression))
            {
                expressions.Add(expression);
                while (TryParseToken(Token.Comma, false))
                {
                    TryParseExpression(out IExpression expression1);
                    expressions.Add(expression1);
                }
            }
            TryParseToken(Token.RoundBracketClose);
            return true;
        }

        private bool TryParseEqualityComparerOperator(out EqualityComparerType? comparerType)
        {
            comparerType = scanner.Current switch
            {
                Token.IsEqual => EqualityComparerType.Equality,
                Token.IsNotEqual => EqualityComparerType.Inequality,
                _ => null
            };
            if (comparerType.HasValue)
            {
                Move();
                return true;
            }
            return false;
        }
        private bool TryParseRelationOperator(out RelationType? relation)
        {
            relation = scanner.Current switch
            {
                Token.LessEqual => RelationType.LessEqual,
                Token.GreaterEqual => RelationType.GreaterEqual,
                Token.LessThan => RelationType.LessThan,
                Token.GreaterThan => RelationType.GreaterThan,
                _ => null
            };
            if (relation.HasValue)
            {
                Move();
                return true;
            }
            return false;
        }
        private bool TryParseAdditiveOperator(out AdditiveOperator? additiveOperator)
        {
            additiveOperator = scanner.Current switch
            {
                Token.Plus=> AdditiveOperator.Add,
                Token.Minus => AdditiveOperator.Subtract,
                _ => null
            };
            if (additiveOperator.HasValue)
            {
                Move();
                return true;
            }
            return false;
        }
        private bool TryParseMultiplicativeOperator(out MultiplicativeOperator? multiplicativeOperator)
        {
            multiplicativeOperator = scanner.Current switch
            {
                Token.Star => MultiplicativeOperator.Multiply,
                Token.Slash => MultiplicativeOperator.Divide,
                _ => null
            };
            if (multiplicativeOperator.HasValue)
            {
                Move();
                return true;
            }
            return false;
        }
        private bool TryParseUnaryOperator(out UnaryOperator? unaryOperator)
        {
            unaryOperator = scanner.Current switch
            {
                Token.Minus => UnaryOperator.Uminus,
                Token.Not => UnaryOperator.LogicalNegation,
                _ => null
            };
            if (unaryOperator.HasValue)
            {
                Move();
                return true;
            }
            return false;
        }
        #endregion expressions
    }
}
