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
            case Stmt.Function s:
                GenFunction(s);
                break;
            case Stmt.ReturnStmt s:
                GenReturn(s);
                break;
            case Stmt.VarDeclaration s:
                GenVarDeclaration(s);
                break;
            default:
                throw new NotImplementedException(stmt.GetType().Name);
        }
    }

    private void GenFunction(Stmt.Function function)
    {
        _identifier = 0;
        
        _ir.Append("define ");
        _ir.Append(function.ReturnType is not null ? function.ReturnType.LLVMName : "void");

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
            case Expr.Variable v:
                _ir.Append($"  ret f32 {v.Name.Text}\n");//TODO
                break;
            default:
                throw new NotImplementedException(r.Expr.GetType().Name);
        }
    }

    private void GenVarDeclaration(Stmt.VarDeclaration varDeclaration)
    {
        
    }

    private void GenVariable(Expr.Variable v)
    {
        
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