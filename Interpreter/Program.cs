using TKOM.ErrorHandler;
using TKOM.Interpreter;
using TKOM.Parser;
using TKOM.Scanner;


var reader = new StreamReader(args[0]);
var errHandler = new ErrorHandler();
var scanner = new Scanner(reader, errHandler);
var noCommentsFilter = new CommentsFilterScanner(scanner);

var parser = new Parser(noCommentsFilter, errHandler);
var interpreter = new Interpreter(errHandler, Console.Out, Console.In);

bool programIsCorrect = parser.TryParse(out TKOM.Node.Program program);
if (programIsCorrect)
{
    program.Accept(interpreter);
}
else
{
    Console.WriteLine("Parsing unsuccessful");
    Console.WriteLine("====================");
}


void PrintScannerResults(Scanner scanner)
{
    uint line = scanner.Position.Line;
    Console.Write($"{line,3}|");
    while (scanner.MoveNext())
    {
        if (scanner.Position.Line != line)
        {
            line = scanner.Position.Line;
            Console.WriteLine();
            Console.Write($"{line,3}|");
        }
        Console.Write($"{scanner.Current.ToString().ToUpper()} ");
    }
}
