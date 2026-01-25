namespace DuxSharp.Lexer;

public class Keyword
{
    public static readonly Dictionary<string, TokenType> Keywords;

    static Keyword()
    {
        Keywords = new Dictionary<string, TokenType>
        {
            ["and"]    = TokenType.And,
            ["else"]   = TokenType.Else,
            ["false"]  = TokenType.False,
            ["for"]    = TokenType.For,
            ["fn"]    = TokenType.Fn,
            ["if"]     = TokenType.If,
            ["or"]     = TokenType.Or,
            ["return"] = TokenType.Return,
            ["true"]   = TokenType.True,
            ["while"]  = TokenType.While,
        };
    }
}