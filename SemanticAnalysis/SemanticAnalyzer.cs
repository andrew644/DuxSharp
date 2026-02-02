using DuxSharp.Parser;

namespace DuxSharp.SemanticAnalysis;

public class SemanticAnalyzer(List<Stmt> stmts) 
{
    private VarScope _functionScope = new VarScope();
    
    public void Analize()
    {
        foreach (var stmt in stmts)
        {
            AnStmt(stmt);
        }
    }
        
    private void AnStmt(Stmt stmt)
    {
        switch (stmt)
        {
            case Stmt.Expression e:
                AnExpression(e);
                break;
            case Stmt.Block b:
                AnBlock(b);
                break;
            case Stmt.Function f:
                AnFunction(f);
                break;
            case Stmt.VarDeclaration v:
                AnVarDeclaration(v);
                break;
            case Stmt.ReturnStmt r:
                AnReturnStmt(r);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void AnExpression(Stmt.Expression e)
    {
        AnExpr(e.Expr);
    }

    private void AnBlock(Stmt.Block b)
    {
        foreach (var stmt in b.Statements)
        {
            AnStmt(stmt);
        }
    }

    private void AnFunction(Stmt.Function f)
    {
        _functionScope = new VarScope();
        foreach (var arg in f.Args)
        {
            //TODO
        }

        foreach (var stmt in f.Body)
        {
            AnStmt(stmt);
        }
    }

    private void AnVarDeclaration(Stmt.VarDeclaration v)
    {
        v.Value.Type = AnExpr(v.Value);
        _functionScope.AddVar(v.Name.Text, v.Value.Type);
    }

    private void AnReturnStmt(Stmt.ReturnStmt r)
    {
        AnExpr(r.Expr);
    }

    private ExprType AnExpr(Expr e)
    {
        switch (e)
        {
            case Expr.Binary b:
                return AnBinary(b);
            case Expr.Unary u:
                return AnUnary(u);
            case Expr.Literal.Integer i:
                return AnInteger(i);
            case Expr.Literal.Float f:
                return AnFloat(f);
            case Expr.Literal.String s:
                return AnString(s);
            case Expr.Grouping g:
                return AnGrouping(g);
            case Expr.Variable v:
                return AnVariable(v);
            case Expr.Assign a:
                return AnAssign(a);
            default:
                throw new NotImplementedException();
        }        
    }

    private ExprType AnBinary(Expr.Binary e)
    {
        AnExpr(e.Left);
        return e.Type = AnExpr(e.Right);
    }

    private ExprType AnUnary(Expr.Unary e)
    {
        return e.Type = AnExpr(e.Right);
    }

    private ExprType AnInteger(Expr.Literal.Integer i)
    {
        i.LiteralValue = i.Value.ToString();
        return ExprType.Ti32;
    }
    
    private ExprType AnFloat(Expr.Literal.Float f)
    {
        f.LiteralValue = f.Value.ToString();
        return ExprType.Tf32;
    }

    private ExprType AnString(Expr.Literal.String s)
    {
        s.LiteralValue = s.Value;
        return ExprType.Tstring;
    }

    private ExprType AnGrouping(Expr.Grouping e)
    {
        return e.Type = AnExpr(e.Expression);
    }

    private ExprType AnVariable(Expr.Variable e)
    {
        ExprType? type = _functionScope.GetVar(e.Name.Text);
        if (type is null) throw new Exception($"Variable '{e.Name.Text}' not found");
        return e.Type = type;
    }

    private ExprType AnAssign(Expr.Assign e)
    {
        return e.Type = AnExpr(e.Value);
    }
}