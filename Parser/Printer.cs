using System.Text;

namespace DuxSharp.Parser;

public static class Printer
{
    public static string Print(Expr expr)
    {
        return expr switch
        {
            Expr.Literal l =>
                l.Value?.ToString() ?? "nil",

            Expr.Variable v =>
                v.Name.Text,

            Expr.Assign a =>
                Parenthesize("assign", a.Name.Text, Print(a.Value)),

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
                s.Name is null
                    ? Parenthesize("var", s.Name.Text)
                    : Parenthesize("var", s.Name.Text, Print(s.Value)),

            Stmt.Block s =>
                Parenthesize("block", s.Statements.Select(Print)),

            Stmt.Function s =>
                Parenthesize(
                    "fn",
                    s.Name.Text,
                    Parenthesize("params", s.Args.Select(p => p.Text)),
                    Parenthesize("body", s.Body.Select(Print))
                ),
            
            Stmt.ReturnStmt s =>
                Parenthesize("return", s.Expr),

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