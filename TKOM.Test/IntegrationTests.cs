using Shouldly;
using System.IO;
using TKOM.Interpreter;
using TKOM.Node;
using TKOM.Parser;
using TKOM.Scanner;
using TKOMTest.Utils;
using Xunit;

namespace TKOMTest
{
    public class IntegrationTests
    {
        [Theory]
        [InlineData("void main(){}")]
        public void Test(string program, string input = "", string output = "")
        {
            OutputCollector outputCollector = new();
            ErrorsCollector errorsCollector = new();

            IScanner scanner = new Scanner(new StringReader(program), errorsCollector);
            IScanner noComments = new CommentsFilterScanner(scanner);
            IParser parser = new Parser(noComments, errorsCollector);
            parser.TryParse(out Program ast);

            Interpreter interpreter = new Interpreter(errorsCollector, outputCollector, new StringReader(input));
            interpreter.Interpret(ast);

            errorsCollector.errorsCount.ShouldBe(0);
            outputCollector.GetOutput().ShouldBe(output);
        }

        [Theory]
        [InlineData(10, 55)]
        public void Fibbonacci(int input, int output)
        {
            string program =
                @"int fibb(int n)
                {
                   if (n <= 0)
                      return 0;
                   if (n == 1)
                      return 1;
                   return fibb(n - 2) + fibb(n - 1);
                }
                void main()
                {
                   int a;
                   read(a);
                   print(fibb(a));
                }";
            Test(program, input.ToString(), output.ToString());
        }

        [Theory]
        [InlineData("1 1", "0 0\n")]
        public void Numbers(string input, string output)
        {
            string program =
                @"
                void main()
                { numbers(); }

                void numbers()
                {
                    int INTMAX;
                    INTMAX = 2147483647;
                    int x;
                    int y;
                    int pierwsze;
                    pierwsze = 0;
                    int maksymalne;
                    maksymalne = 0;
                    int max;
                    read(x);
                    read(y);
                    int i;
                    i = x;
                    while (i < y)
                    {
                        int z;
                        z = i;
                        while (z != 1)
                        {
                            int help;
                            help = z / 2;
                            if (help * 2 == z)
                            // if(z % 2 == 0)
                            {
                                z = z / 2;
                            }
                            else if ((INTMAX - 1) / 3 > z)
                            {
                                z = 3 * z + 1;
                            }
                            else
                            {
                                print(i);
                                print(""?\n"");
                                // print(""%d?\n"", i);
                                return;
                            }
                            pierwsze = pierwsze + 1;
                        }
                        if (pierwsze > maksymalne)
                        {
                            maksymalne = pierwsze;
                            max = i;
                        }
                        pierwsze = 0;
                        i = i + 1;
                    }
                    print(max);
                    print("" "");
                    print(maksymalne);
                    print(""\n"");
                }";
            Test(program, input, output);
        }

        [Fact]
        public void Choinka()
        {
            string program =
                @"
                void main()
                { choinka(); }
                int choinka()
                {
                    int n;
                    read(n);
                    int i;
                    i = 0;
                    while (i < n)
                    {
                        int k;
                        k = n - i;
                        int l;
                        l = n + i;
                        int o;
                        o = 0;
                        while (o < 2 * n)
                        {
                            if(o < k || o > l)
                            {
                                print("" "");
                            }
                            else
                            {
                                print(""*"");
                            }
                            o = o + 1;
                        }
                        print(""\n"");
                        i = i + 1;
                    }
                    i = 0;

                    while (i < 3)
                    {
                        int k;
                        k = n - 1;
                        int o;
                        o = 0;
                        while (o < n + 2)
                        {
                            if (o < k)
                            {
                                print("" "");
                            }
                            else
                            {
                                print(""*"");
                            }
                            o = o + 1;
                        }
                        print(""\n"");
                        i = i + 1;
                    }
                    return 0;
                }";

            string input = "10";
            string output =
                "          *         \n" +
                "         ***        \n" +
                "        *****       \n" +
                "       *******      \n" +
                "      *********     \n" +
                "     ***********    \n" +
                "    *************   \n" +
                "   ***************  \n" +
                "  ***************** \n" +
                " *******************\n" +
                "         ***\n" +
                "         ***\n" +
                "         ***\n";
            Test(program, input, output);
        }
    }
}
