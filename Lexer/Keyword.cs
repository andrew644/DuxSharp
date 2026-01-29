namespace DuxSharp.Lexer;

public static class Keyword
{
    public static readonly Dictionary<string, TokenType> Keywords;

    static Keyword()
    {
        Keywords = new Dictionary<string, TokenType>
        {
            ["and"]    = TokenType.And,
            ["or"]     = TokenType.Or,
            ["if"]     = TokenType.If,
            ["else"]   = TokenType.Else,
            ["true"]   = TokenType.True,
            ["false"]  = TokenType.False,
            ["for"]    = TokenType.For,
            ["fn"]    = TokenType.Fn,
            ["return"] = TokenType.Return,
            ["defer"] = TokenType.Defer,
        };
    }
}