using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public static class Precedence
{
    public const int None = 0; //TODO use enum
    public const int Assignment = None + 1; // = += -= *= /=
    public const int LogicalOr = Assignment + 1;
    public const int LogicalAnd = LogicalOr + 1;
    public const int Equality = LogicalAnd + 1; // == !=
    public const int Comparison = Equality + 1; // < <= > >=
    public const int Term = Comparison + 1; // + -
    public const int Factor = Term + 1; // * / mod
    public const int Unary = Factor + 1; // ! -
    public const int Call = Unary + 1; // () . []
    public const int Primary = Call + 1; 

    public static int GetPrecedence(Token token) => token.Type switch
    {
        TokenType.Equals 
            or TokenType.PlusEquals
            or TokenType.MinusEquals
            or TokenType.StarEquals
            or TokenType.SlashEquals => Assignment,
        TokenType.Or => LogicalOr,
        TokenType.And => LogicalAnd,
        TokenType.DoubleEquals 
            or TokenType.ExclamationEquals => Equality,
        TokenType.Greater
            or TokenType.Less
            or TokenType.GreaterEquals
            or TokenType.LessEquals => Comparison,
        TokenType.Plus 
            or TokenType.Minus => Term,
        TokenType.Star 
            or TokenType.Slash
            or TokenType.Percent => Factor,
        TokenType.Exclamation => throw new Exception("We shouldn't get here, this is unary !"),
        _ => None
    };
}