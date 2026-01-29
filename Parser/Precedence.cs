using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public static class Precedence
{
    public const int Assignment = 1;
    public const int Term = 2;
    public const int Factor = 3;
    public const int Unary = 4;
    public const int Primary = 5;

    public static int GetPrecedence(Token token) => token.Type switch
    {
        TokenType.Equals => Precedence.Assignment,
        TokenType.Plus or TokenType.Minus => Precedence.Term,
        TokenType.Star or TokenType.Slash => Precedence.Factor,
        _ => 0
    };
}