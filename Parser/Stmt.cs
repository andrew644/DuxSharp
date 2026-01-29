using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public abstract record Stmt
{
    public record Expression(Expr Expr) : Stmt;
    public record Block(List<Stmt> Statements) : Stmt;
    public record Function(Token Name, List<Token> Args, List<Stmt> Body, Token? ReturnType) : Stmt;
    public record VarDeclaration(Token Name, Expr Value) : Stmt;
}