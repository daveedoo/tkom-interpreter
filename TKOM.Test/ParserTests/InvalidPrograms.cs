using Shouldly;
using TKOM.Node;
using TKOM.Parser;
using Xunit;

namespace TKOMTest.ParserTests
{
    public class InvalidPrograms : ParserTests
    {
        public static TheoryData<string> invalidPrograms => new TheoryData<string>
        {
            "",                                 // empty program
            "int main",                         // incomplete function
            "int return() {}",                  // keyword as identifier
            "int main() {} &&",                 // illegal token at the end
            "int main ( )",                     // no block
            "int main(int) {}",                 // incomplete parameter
            "int main(int a {}",                // not closed parameters list
            "int main(int a,) {}",              // parameter with additional comma
            "int main(void a) {}",              // void type parameter
            "int main() { int; }",              // incomplete declaration
            "int main() { a =; }",              // incomplete assignment
            "int main() { a =5 }",              // instruction without semicolon
            "int main() { a = b = c; }",        // multiple assignment
            "int main() { foo(; }",             // incomplete function call
            "int main() { foo; }",              // incomplete function call
            "int main() { foo(2 int); }",       // function call with incorrect argument
            "int main() { throw; }",            // incomplete throw
            "int main() { a = b || ; }",        // incomplete logical or
            "int main() { a = b && ; }",        // incomplete logical and
            "int main() { a = b == ; }",        // incomplete equality operator
            "int main() { a = b != ; }",        // incomplete inequality operator
            "int main() { a = b >= ; }",        // incomplete relation operator
            "int main() { a = b + ; }",         // incomplete additive operator
            "int main() { a = b * ; }",         // incomplete multiplicative operator
            "int main() { a = (b * c; }",       // incomplete brackets operator
        };


        [Theory]
        [MemberData(nameof(invalidPrograms))]
        public void InvalidProgram_ShouldSetASTToNull(string program)
        {
            IParser parser = buildParser(program);

            parser.TryParse(out Program ast);

            ast.ShouldBeNull();
        }

        [Theory]
        [MemberData(nameof(invalidPrograms))]
        public void InvalidProgram_ShouldReturnFalse(string program)
        {
            IParser parser = buildParser(program);

            bool parsed = parser.TryParse(out Program _);

            parsed.ShouldBeFalse();
        }

        [Theory(Skip = "errors and warnings handling not implemented yet")]
        [MemberData(nameof(invalidPrograms))]
        public void InvalidProgram_ShouldThrowError(string program)
        {
            IParser parser = buildParser(program);

            parser.TryParse(out Program _);

            errorHandler.errorCount.ShouldBeGreaterThan(0);
        }
    }
}
