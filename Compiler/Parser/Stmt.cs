using Compiler.Lexer;

namespace Compiler.Parser;

public abstract record Stmt
{
    public record Expression(Expr Expr) : Stmt;
    public record Block(List<Stmt> Statements) : Stmt;

    public record Function(Token Name, List<(Token name, ExprType? type)> Args, Stmt Body, ExprType? ReturnType) : Stmt;
    public record VarDeclaration(Token Name, Expr? Value, ExprType? Type = null) : Stmt;
    public record ReturnStmt(Expr Expr) : Stmt;
    public record IfStmt(Expr Condition, Stmt Body, Stmt? Else) : Stmt;
    public record ForStmt(Stmt? Start, Expr? Condition, Expr? Iteration, Stmt Body) : Stmt;
    public record PrintfStmt(Expr.Literal.String Format, List<Expr> Args) : Stmt;
}