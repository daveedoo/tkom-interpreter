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
        private IErrorHandler errorHandler;

        public Parser(IScanner scanner, IErrorHandler errorHandler)
        {
            this.scanner = scanner;
            this.errorHandler = errorHandler;
            scanner.MoveNext();
        }

        public bool TryParse(out Program program)
        {
            IList<FunctionDefinition> functions = new List<FunctionDefinition>();
            while (TryParseFunctionDefinition(out FunctionDefinition function))
            {
                functions.Add(function);
            }
            if (functions.Count == 0)
            {
                program = null;
                return false;
            }

            program = new Program(functions);
            return true;
        }

        private bool TryParseFunctionDefinition(out FunctionDefinition funDef)
        {
            funDef = null;
            if (!TryCastTokenToType(scanner.Current, out Type? type))
                return false;
            scanner.MoveNext();
            if (scanner.Current != Token.Identifier)
                return false;

            var parameters = new List<Parameter>();
            funDef = new FunctionDefinition(type.Value, scanner.StringValue, parameters);
            return true;
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
