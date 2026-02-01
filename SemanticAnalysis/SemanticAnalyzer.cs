using DuxSharp.Parser;

namespace DuxSharp.SemanticAnalysis;

public class SemanticAnalyzer(List<Stmt> stmts) 
{
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
        AnExpr(v.Value);
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

    private ExprType AnBinary(Expr.Binary b)
    {
        AnExpr(b.Left);
        return AnExpr(b.Right);
    }

    private ExprType AnUnary(Expr.Unary u)
    {
        return AnExpr(u.Right);
    }

    private ExprType AnInteger(Expr.Literal.Integer i)
    {
        return ExprType.Ti32;
    }
    
    private ExprType AnFloat(Expr.Literal.Float f)
    {
        return ExprType.Tf32;
    }

    private ExprType AnString(Expr.Literal.String s)
    {
        return ExprType.Tstring;
    }

    private ExprType AnGrouping(Expr.Grouping g)
    {
        return AnExpr(g.Expression);
    }

    private ExprType AnVariable(Expr.Variable v)
    {
        return ExprType.Ti32; //TODO we need to get this from the declaration
    }

    private ExprType AnAssign(Expr.Assign a)
    {
        return AnExpr(a);
    }
}