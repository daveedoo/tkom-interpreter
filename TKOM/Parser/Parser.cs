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



        public bool TryParse(out Program program)                                           // program             : { function }
        {
            program = null;
            if (!Move())
                return false;

            IList<FunctionDefinition> functions = new List<FunctionDefinition>();
            while (scanner.Current != Token.EOF)
            {
                if (!TryParseFunctionDefinition(out FunctionDefinition function))
                    return false;
                functions.Add(function);
            }
            //if (functions.Count == 0)
            //{
            //    errorHandler.HandleError("Program should should contain at least main function.");
            //    return false;
            //}

            program = new Program(functions);
            return true;
        }

        #region helper methods
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
        private bool Move()
        {
            if (!scanner.MoveNext())
            {
                //errorHandler.HandleError(new LexLocation(scanner.Position, scanner.Position), "Unexpected EOF.");   // TODO: add HandleError(Position, string)
                return false;
            }
            return true;
        }
        #endregion // helper methods

        private bool TryParseFunctionDefinition(out FunctionDefinition funDef)              // function            : ( "void" | type ) IDENTIFIER param_list block
        {
            funDef = null;
            if (!TryParseFunctionReturnType(out Type? type, "Expected function return type.") ||
                !TryParseToken(Token.Identifier, out string functionName) ||
                !TryParseParameters(out IList<Parameter> parameters) ||
                !TryParseBlock(out Block block))
                return false;

            funDef = new FunctionDefinition(type.Value, functionName, parameters, block);
            return true;
        }
        private bool TryParseFunctionReturnType(out Type? type, string errorMessage)                             // ( "void" | type )
        {
            if (TryParseTypeToken(out type))
                return true;
            if (TryParseToken(Token.Void))
            {
                type = Type.Void;
                return true;
            }
            errorHandler.Error(errorMessage);
            return false;

        }
        private bool TryParseTypeToken(out Type? type)                                      // type                : "int"
        {
            bool intParsed = TryParseToken(Token.Int);
            type = intParsed ? Type.IntType : null;
            return intParsed;
        }
        private bool TryParseParameters(out IList<Parameter> parameters)                    // param_list           : "(" [ type IDENTIFIER { "," type IDENTIFIER } ] ")"
        {
            parameters = null;
            if (!TryParseToken(Token.RoundBracketOpen)) // TODO: brackety na poziom funkcji
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

        private bool TryParseBlock(out Block block)                                         // block                : "{" { statement } "}"
        {
            block = null;
            var statements = new List<IStatement>();
        
            if (!TryParseToken(Token.CurlyBracketOpen))
                return false;
            while (!TryParseToken(Token.CurlyBracketClose))     // TODO: tryparsestatement
            {
                if (TryParseStatement(out IStatement statement))
                    statements.Add(statement);
                else
                    return false;
            }
            // TODO: tryparse curly bracket close
            block = new Block(statements);
            return true;
        }
        private bool TryParseStatement(out IStatement statement)                            // statement           : simple_statement ";" | block_statement
        {
            if (TryParseSimpleStatement(out statement) ||
                TryParseBlockStatement(out statement))
                return true;
            return false;
        }
        private bool TryParseSimpleStatement(out IStatement statement)                      // statement           : simple_statement ";"
        {
            statement = null;
            if (TryParseDeclarationsWithOptionalAssignments(out Declaration declaration))   // simple_statement    : declaration
                statement = declaration;
            else if (TryParseReturnStatement(out Return returnStatement))                   //                     | return
                statement = returnStatement;
            else if (TryParseThrowStatement(out Throw throwStatement))                      //                     | throw
                statement = throwStatement;
            else if (!TryParse_Assignment_FunctionCall(out statement))                      //                     | assignment | function_call
                return false;

            // ";"
            if (!TryParseToken(Token.Semicolon))
            {
                statement = null;
                return false;
            }
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
            if (!TryParseTypeToken(out Type? type) ||
                !TryParseToken(Token.Identifier, out string identifier))
            {
                statement = null;
                return false;
            }
            statement = new Declaration(type.Value, identifier);
            return true;
        }
        private bool TryParseReturnStatement(out Return statement)                          // return              : "return" [ expression ]
        {
            statement = null;
            if (!TryParseToken(Token.Return))
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
            if (!TryParseToken(Token.Identifier, out string identifier))
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
            if (!TryParseToken(Token.Equals) ||
                !TryParseExpression(out IExpression expression))
                return false;
            assignment = new Assignment(identifier, expression);
            return true;
        }
        #endregion

        #region block statements
        private bool TryParseIfStatement(out If ifStatement)                                // if                  : "if" "(" expression ")" statement [ "else" statement ]
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
                if (!TryParseStatement(out IStatement elseStmt))
                    return false;
                ifStatement = new If(condition, stmt, elseStmt);
                return true;
            }

            ifStatement = new If(condition, stmt);
            return true;
        }
        private bool TryParseWhileStatement(out While whileStatement)                       // while               : "while" "(" expression ")" statement
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
        
        private bool TryParseThrowStatement(out Throw statement)                            // throw               : "throw" "Exception" "(" expression ")"
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
        private bool TryParseTryCatchFinallyStatement(out TryCatchFinally tcfStatement)     // try_catch_finally   : "try" statement catch { catch } [ "finally" statement]
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
        private bool TryParseCatch(out Catch catchBlock)                                    // catch               : "catch" "Exception" IDENTIFIER [ "when" expression ] statement
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
            else
            {
                if (!TryParseStatement(out IStatement statement))
                    return false;
                catchBlock = new Catch(variable, statement);
                return true;
            }
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
            while (TryParseToken(Token.Or))
            {
                if (!TryParseLogicalAnd(out IExpression right))
                {
                    expression = null;
                    return false;
                }
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
            while (TryParseToken(Token.And))
            {
                if (!TryParseEqualityComparers(out IExpression right))
                {
                    expression = null;
                    return false;
                }
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
                {
                    expression = null;
                    return false;
                }
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
                if (!TryParseAdditive(out IExpression right))
                {
                    expression = null;
                    return false;
                }
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
                {
                    expression = null;
                    return false;
                }
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
                {
                    expression = null;
                    return false;
                }
                expression = new Multiplicative(expression, multiplicativeOperator.Value, unary);
            }
            return true;
        }
        private bool TryParseUnary(out IExpression unary)                                    // unar                : [ "-" | "!" ] ( ( "(" expression ")" ) atomic )
        {
            unary = null;
            TryParseUnaryOperator(out UnaryOperator? unaryOperator);

            if (TryParseToken(Token.RoundBracketOpen))
            {
                if (!TryParseExpression(out IExpression expression) ||
                    !TryParseToken(Token.RoundBracketClose))
                    return false;
                unary = expression;
            }
            else if (!TryParseAtomic(out unary))
                return false;

            if (unaryOperator.HasValue)
                unary = new Unary(unaryOperator.Value, unary);
            return true;
        }
        private bool TryParseAtomic(out IExpression expression)                             // atomic              : const | IDENTIFIER | function_call | string
        {
            expression = null;
            if (TryParseToken(Token.IntConst, out int? intConst))
                expression = new IntConst(intConst.Value);
            else if (TryParseToken(Token.Identifier, out string identifier))
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

        private bool TryParseFunctionArguments(out IList<IExpression> expressions)          // [ expression { "," expression } ] ")"
        {
            expressions = null;
            if (!TryParseToken(Token.RoundBracketOpen))
                return false;

            expressions = new List<IExpression>();
            if (TryParseExpression(out IExpression expression))
            {
                expressions.Add(expression);
                while (TryParseToken(Token.Comma))
                {
                    if (!TryParseExpression(out IExpression expression1))
                    {
                        expressions = null;
                        return false;
                    }
                    expressions.Add(expression1);
                }
            }
            if (!TryParseToken(Token.RoundBracketClose))
            {
                expressions = null;
                return false;
            }
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
