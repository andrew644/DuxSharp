using Compiler.Lexer;

namespace Compiler.Parser;

public abstract record Expr()
{
    public ExprType? Type { get; set; }
    public string? LiteralValue { get; set; }
    public record Binary(Expr Left, Token Op, Expr Right) : Expr;

    public record Unary(Token Op, Expr Right) : Expr;

    public abstract record Literal() : Expr
    {
        public record Integer(long Value) : Literal;
        public record Float(double Value) : Literal;
        public record String(string Value) : Literal;
    }
    public record Grouping(Expr Expression) : Expr;
    public record Variable(Token Name) : Expr;
    public record FunctionCall(Token Name, List<Expr> Args) : Expr;
    public record ArrayIndex(Token Name, Expr Index) : Expr;
    public record Assign(Expr LValue, Expr Value, Token Op) : Expr; 
}