using TKOM.ErrorHandler;
using TKOM.Scanner;

string programFilename = "program.txt";
string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
string path = Path.Combine(projectDir, programFilename);
StreamReader reader = new StreamReader(path);

ErrorHandler handler = new ErrorHandler();
Scanner scanner = new Scanner(reader, handler);

uint line = scanner.LineNumber;
Console.Write($"{line,3}|");
while (scanner.MoveNext())
{
    if (scanner.LineNumber != line)
    {
        line = scanner.LineNumber;
        Console.WriteLine();
        Console.Write($"{line, 3}|");
    }
    Console.Write($"{scanner.Current.ToString().ToUpper()} ");
}
