using System.Text;
using DuxSharp.Parser;

namespace DuxSharp.CodeGeneration;

public class CodeGen(List<Stmt> ast)
{
    private readonly StringBuilder _ir = new StringBuilder();
    private int _identifier;

    public string Generate()
    {
        foreach (var stmt in ast)
        {
            GenStmt(stmt);
        }

        return _ir.ToString();
    }

    private void GenStmt(Stmt stmt)
    {
        switch (stmt)
        {
            case Stmt.Function f:
                GenFunction(f);
                break;
            case Stmt.ReturnStmt r:
                GenReturn(r);
                break;
            default:
                throw new NotImplementedException(stmt.GetType().Name);
        }
    }

    private void GenFunction(Stmt.Function function)
    {
        _identifier = 0;
        
        _ir.Append("define ");
        if (function.ReturnType != null)
        {
            _ir.Append(function.ReturnType.Text);
        }
        else
        {
            _ir.Append("void");
        }

        _ir.Append($" @{function.Name.Text}(");
        //TODO args
        _ir.Append(") {\n");
        foreach (var stmt in function.Body)
        {
            GenStmt(stmt);
        }
        _ir.Append("}\n");
    }

    private void GenReturn(Stmt.ReturnStmt r)
    {
        switch (r.Expr)
        {
            //TODO maybe save the return type in the ReturnStmt
            case Expr.Literal.Integer i:
                _ir.Append($"  ret i32 {i.Value}\n");
                break;
            case Expr.Literal.Float f:
                _ir.Append($"  ret f32 {f.Value}\n");
                break;
            default:
                throw new NotImplementedException(r.Expr.GetType().Name);
        }
    }

    private int GenExpr(Expr expr)
    {
        switch (expr)
        {
            case Expr.Literal l:
                break;
            default:
                throw new NotImplementedException(expr.GetType().Name);
        }

        _identifier++;
        
        return _identifier;
    }
}