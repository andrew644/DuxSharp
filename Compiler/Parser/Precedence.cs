using Compiler.Lexer;

namespace Compiler.Parser;

public static class Precedence
{
    public static int GetPrecedence(Token token) => token.Type switch
    {
        TokenType.Equals 
            or TokenType.PlusEquals
            or TokenType.MinusEquals
            or TokenType.StarEquals
            or TokenType.SlashEquals => (int)PrecedenceEnum.Assignment,
        TokenType.Or => (int)PrecedenceEnum.LogicalOr,
        TokenType.And => (int)PrecedenceEnum.LogicalAnd,
        TokenType.DoubleEquals 
            or TokenType.ExclamationEquals => (int)PrecedenceEnum.Equality,
        TokenType.Greater
            or TokenType.Less
            or TokenType.GreaterEquals
            or TokenType.LessEquals => (int)PrecedenceEnum.Comparison,
        TokenType.Plus 
            or TokenType.Minus => (int)PrecedenceEnum.Term,
        TokenType.Star 
            or TokenType.Slash
            or TokenType.Percent => (int)PrecedenceEnum.Factor,
        TokenType.Exclamation => throw new Exception("We shouldn't get here, this is unary !"),
        _ => (int)PrecedenceEnum.None
    };
}