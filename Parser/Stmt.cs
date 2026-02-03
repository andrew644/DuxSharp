using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public abstract record Stmt
{
    public record Expression(Expr Expr) : Stmt;
    public record Block(List<Stmt> Statements) : Stmt;

    public record Function(Token Name, List<(string name, ExprType? type)> Args, List<Stmt> Body, ExprType? ReturnType) : Stmt;
    public record VarDeclaration(Token Name, Expr Value) : Stmt;
    public record ReturnStmt(Expr Expr) : Stmt;
    public record IfStmt(Expr Condition, Stmt Body, Stmt? Else) : Stmt;
    public record ForStmt(Stmt? Start, Expr? Condition, Expr? Iteration, Stmt Body) : Stmt;
}