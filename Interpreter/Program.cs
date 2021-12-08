using TKOM.ErrorHandler;
using TKOM.Scanner;


using StreamReader reader = new(args[0]);
IErrorHandler handler = new ErrorHandler();
Scanner scanner = new(reader, handler);

uint line = scanner.Position.Line;
Console.Write($"{line,3}|");
while (scanner.MoveNext())
{
    if (scanner.Position.Line != line)
    {
        line = scanner.Position.Line;
        Console.WriteLine();
        Console.Write($"{line, 3}|");
    }
    Console.Write($"{scanner.Current.ToString().ToUpper()} ");
}
