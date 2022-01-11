using TKOM.ErrorHandler;
using TKOM.Interpreter;
using TKOM.Parser;
using TKOM.Scanner;


var reader = new StreamReader(args[0]);
var handler = new ErrorHandler();
var scanner = new Scanner(reader, handler);
var noCommentsFilter = new CommentsFilterScanner(scanner);

var parser = new Parser(noCommentsFilter, handler);
var interpreter = new Interpreter(handler, Console.Out);

bool programIsCorrect = parser.TryParse(out TKOM.Node.Program program);
if (programIsCorrect)
{
    Console.WriteLine("Parsing succesfull");
    Console.WriteLine("==================");
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
