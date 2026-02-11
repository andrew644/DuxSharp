using System.Text;

namespace Compiler.Parser;

public static class Printer
{
    public static string Print(Expr expr)
    {
        return expr switch
        {
            Expr.Literal.Integer i =>
                i.Value.ToString(),
            Expr.Literal.Float f =>
                f.Value.ToString(),
            Expr.Literal.String s =>
                s.Value,

            Expr.Variable v =>
                v.Name.Text,

            Expr.FunctionCall e =>
                Parenthesize(e.Name.Text, e.Args.Select(Print)),
            
            Expr.ArrayIndex e =>
                Parenthesize("[]", e.Name.Text, Print(e.Index)),
            
            Expr.Assign a =>
                Parenthesize("assign", Print(a.LValue), Print(a.Value), a.Op.Text),

            Expr.Unary u =>
                Parenthesize(u.Op.Text, Print(u.Right)),

            Expr.Binary b =>
                Parenthesize(
                    b.Operator.Text,
                    Print(b.Left),
                    Print(b.Right)
                ),

            Expr.Grouping g =>
                Print(g.Expression),

            _ => throw new NotImplementedException(expr.GetType().Name)
        };
    }

    public static string Print(Stmt stmt)
    {
        return stmt switch
        {
            Stmt.Expression s =>
                Print(s.Expr),

            Stmt.VarDeclaration s =>
                Parenthesize("var", s.Name.Text, s.Value is not null ? Print(s.Value) : "void"),

            Stmt.Block s =>
                Parenthesize("block", s.Statements.Select(Print)),

            Stmt.Function s =>
                Parenthesize(
                    "fn",
                    s.Name.Text,
                    Parenthesize("params", s.Args.Select(p => $"{p.name.Text}: {p.type.LLVMName}")),
                    Parenthesize("returns", s.ReturnType is not null ? s.ReturnType.LLVMName : "void"),
                    Parenthesize("body", s.Body.Select(Print))
                ),
            
            Stmt.IfStmt s =>
                Parenthesize(
                    "if",
                    Parenthesize("condition", s.Condition),
                    Parenthesize("body",Print(s.Body)),
                    Parenthesize("else", s.Else is not null ? s.Else : "No Else")
                ),
            
            Stmt.ForStmt s =>
                Parenthesize(
                    "for",
                    Parenthesize("start", s.Start is not null ? s.Start : "void"),
                    Parenthesize("condition", s.Condition is not null ? s.Condition : "void"),
                    Parenthesize("iteration", s.Iteration is not null ? s.Iteration : "void"),
                    Parenthesize("body", Print(s.Body))
                ),
            
            Stmt.ReturnStmt s =>
                Parenthesize("return", Print(s.Expr)),
            
            Stmt.PrintfStmt s =>
                Parenthesize("prinft", s.Format, s.Args.Select(Print)),

            _ => throw new NotImplementedException(stmt.GetType().Name)
        };
    }

    private static string Parenthesize(string name, params object[] parts)
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(name);

        foreach (var part in parts)
        {
            sb.Append(" ");

            if (part is string s)
                sb.Append(s);
            else if (part is IEnumerable<string> list)
                sb.Append(string.Join(" ", list));
            else
                sb.Append(part);
        }

        sb.Append(")");
        return sb.ToString();
    }
}