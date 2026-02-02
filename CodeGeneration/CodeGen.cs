using System.Text;
using DuxSharp.Lexer;
using DuxSharp.Parser;
using DuxSharp.SemanticAnalysis;

namespace DuxSharp.CodeGeneration;

public class CodeGen(List<Stmt> ast)
{
    private readonly StringBuilder _ir = new StringBuilder();
    private int _identifier = 1;
    private VarScope _functionScope = new VarScope();

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
            case Stmt.Expression s:
                GenExpressionStmt(s);
                break;
            default:
                throw new NotImplementedException(stmt.GetType().Name);
        }
    }

    private void GenFunction(Stmt.Function function)
    {
        _identifier = 1;
        _functionScope = new VarScope();
        
        _ir.Append("define ");
        _ir.Append(function.ReturnType is not null ? function.ReturnType.LLVMName : "void");

        _ir.Append($" @{function.Name.Text}(");
        //TODO args
        _ir.AppendLine(") {");
        foreach (var stmt in function.Body)
        {
            GenStmt(stmt);
        }
        _ir.AppendLine("}");
    }

    private void GenReturn(Stmt.ReturnStmt r)
    {
        switch (r.Expr)
        {
            case Expr.Literal.Integer i:
                _ir.AppendLine($"  ret i32 {i.Value}");
                break;
            case Expr.Literal.Float f:
                _ir.AppendLine($"  ret f32 {f.Value}");
                break;
            /*
            case Expr.Variable v:
                ExprType? type = _functionScope.GetVar(v.Name.Text);
                if (type is null)
                {
                    throw new Exception($"Variable {v.Name.Text} not found");
                }
                _ir.AppendLine($"  %{_identifier} = load {type.LLVMName}, ptr %{v.Name.Text}");
                _ir.AppendLine($"  ret {type.LLVMName} %{_identifier}");
                break;
                */
            default:
                int id = GenExpr(r.Expr);
                _ir.AppendLine($"  ret {r.Expr.Type.LLVMName} %{id}");
                break;
        }
    }

    private void GenVarDeclaration(Stmt.VarDeclaration vd)
    {
        _functionScope.AddVar(vd.Name.Text, vd.Value.Type!);
        _ir.Append($"  %{vd.Name.Text} = alloca {vd.Value.Type.LLVMName}\n");
        if (vd.Value.LiteralValue is not null)
        {
            _ir.AppendLine($"  store {vd.Value.Type.LLVMName} {vd.Value.LiteralValue}, ptr %{vd.Name.Text}");
            return;
        }
        
        int id = GenExpr(vd.Value);
        _ir.AppendLine($"  store {vd.Value.Type.LLVMName} %{id}, ptr %{vd.Name.Text}");
    }
    
    private void GenExpressionStmt(Stmt.Expression e)
    {
        GenExpr(e.Expr);
    }
    
    private int GenExpr(Expr expr)
    {
        switch (expr)
        {
            case Expr.Variable e:
                return GenVariable(e);
            case Expr.Assign e:
                return GenAssign(e);
            case Expr.Binary e:
                return GenBinary(e);
            default:
                throw new NotImplementedException(expr.GetType().Name);
        }
    }
    private int GenVariable(Expr.Variable v)
    {
        ExprType? type = _functionScope.GetVar(v.Name.Text);
        if (type is null)
        {
            throw new Exception($"Variable {v.Name.Text} not found");
        }

        _ir.AppendLine($"  %{_identifier} = load {type.LLVMName}, ptr %{v.Name.Text}");
        return _identifier++;
    }

    private int GenAssign(Expr.Assign e)
    {
        ExprType? type = _functionScope.GetVar(e.Name.Text);
        if (type is null) throw new Exception($"var {e.Name.Text} not found");
        if (e.Value.LiteralValue is not null)
        {
            _ir.AppendLine($"  store {type.LLVMName} {e.Value.LiteralValue}, ptr %{e.Name.Text}");
            return -1; //TODO allow assignment to be used as an expression?
        }
        
        int identifier = GenExpr(e.Value);
        _ir.AppendLine($"  store {type.LLVMName} %{identifier}, ptr %{e.Name.Text}");
        return identifier;
    }

    private int GenBinary(Expr.Binary e)
    {
        string leftValue = e.Left.LiteralValue ?? $"%{GenExpr(e.Left)}";
        string rightValue = e.Right.LiteralValue ?? $"%{GenExpr(e.Right)}";
        switch (e.Operator.Type)
        {
            case TokenType.Plus:
                _ir.AppendLine($"  %{_identifier} = add nsw {e.Type.LLVMName} {leftValue}, {rightValue}");
                break;
            case TokenType.Minus:
                _ir.AppendLine($"  %{_identifier} = sub nsw {e.Type.LLVMName} {leftValue}, {rightValue}");
                break;
            case TokenType.Star:
                _ir.AppendLine($"  %{_identifier} = mul nsw {e.Type.LLVMName} {leftValue}, {rightValue}");
                break;
            case TokenType.Slash:
                _ir.AppendLine($"  %{_identifier} = sdiv {e.Type.LLVMName} {leftValue}, {rightValue}");
                break;
            default:
                throw new NotImplementedException();
        }

        return _identifier++;
    }
}