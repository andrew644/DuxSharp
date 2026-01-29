using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public abstract record Expr
{
    public record Binary(Expr Left, Token Operator, Expr Right) : Expr;
    public record Unary(Token Op, Expr Right) : Expr;
    public record Literal(object? Value) : Expr;
    public record Grouping(Expr Expression) : Expr;
    public record Variable(Token Name) : Expr;
    public record Assign(Token Name, Expr Value) : Expr; 
}