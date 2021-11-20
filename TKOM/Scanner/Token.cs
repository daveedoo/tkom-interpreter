namespace TKOM.Scanner
{
    public enum Token
    {
        Error,
        Identifier,         // [a-zA-Z][a-zA-Z0-9]*
        IntConst,           // 0|([1-9][0-9]*)
        String,             // ".*"
        Comment,            // //.*\n
        // Keywords
        Void, Int,
        Return,
        If, Else, While,
        Read, Print,
        Try, Catch, Finally, Throw, When, Exception,
        // Operators
        RoundBracketOpen, RoundBracketClose,
        CurlyBracketOpen, CurlyBracketClose,
        Minus, Plus, Star, Slash,
        Or, And,
        LessThan, GreaterThan, Equals, Not,
        Semicolon, Comma, Dot
    }
}
