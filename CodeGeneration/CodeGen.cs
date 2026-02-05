using System.Text;
using DuxSharp.Lexer;
using DuxSharp.Parser;
using DuxSharp.SemanticAnalysis;

namespace DuxSharp.CodeGeneration;

public class CodeGen(List<Stmt> ast)
{
    private readonly StringBuilder _ir = new StringBuilder();
    private readonly StringBuilder _irHeader = new StringBuilder();
    private readonly Dictionary<string, int> _stringLiterals = new Dictionary<string, int>();
    private int _identifier = 1;
    private VarScope _functionScope = new VarScope();

    public string Generate()
    {
        _irHeader.AppendLine("target triple = \"x86_64-pc-linux-gnu\"");
        
        foreach (var stmt in ast)
        {
            GenStmt(stmt);
        }
        
        StringBuilder stringLiterals = new StringBuilder();

        //TODO move this to a class
        foreach (var (stringLiteral, stringId) in _stringLiterals)
        {
            string replaced = stringLiteral.Replace("\\n", "\\0A") + "\\00";
            int backslash = replaced.Count(c => c == '\\');
            int size = replaced.Length - 2 * backslash;
            stringLiterals.AppendLine(
                $"@.str.{stringId} = constant [{size} x i8] c\"{replaced}\"");
        }
        stringLiterals.AppendLine();
        
        return _irHeader.ToString()
               + stringLiterals.ToString()
               + _ir.ToString()
               + "\n\ndeclare i32 @printf(ptr, ...)\n"; // import printf from c
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
            case Stmt.Block s:
                GenBlock(s);
                break;
            case Stmt.Expression s:
                GenExpressionStmt(s);
                break;
            case Stmt.IfStmt s:
                GenIfStmt(s);
                break;
            case Stmt.ForStmt s:
                GenForStmt(s);
                break;
            case Stmt.PrintfStmt s:
                GenPrintfStmt(s);
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

    private void GenBlock(Stmt.Block block)
    {
        foreach(var stmt in block.Statements)
        {
            GenStmt(stmt);
        }
    }

    private void GenReturn(Stmt.ReturnStmt r)
    {
        string value = r.Expr.LiteralValue ?? $"%{GenExpr(r.Expr)}";
        _ir.AppendLine($"  ret {r.Expr.Type.LLVMName} {value}");
    }

    private void GenVarDeclaration(Stmt.VarDeclaration vd)
    {
        _functionScope.AddVar(vd.Name.Text, vd.Value.Type!);
        _ir.Append($"  %{vd.Name.Text} = alloca {vd.Value.Type.LLVMName}\n");
        string value = vd.Value.LiteralValue ?? $"%{GenExpr(vd.Value)}";
        _ir.AppendLine($"  store {vd.Value.Type.LLVMName} {value}, ptr %{vd.Name.Text}");
    }

    private void GenIfStmt(Stmt.IfStmt ifStmt)
    {
        int conditionId = GenExpr(ifStmt.Condition);
        string jumpBody = $"if_body{_identifier++}";
        string jumpEnd = $"if_end{_identifier++}";
        if (ifStmt.Else is not null)
        {
            string jumpElse = $"else_body{_identifier++}";
            _ir.AppendLine($"  br i1 %{conditionId}, label %{jumpBody}, label %{jumpElse}");
            _ir.AppendLine($"{jumpBody}:");
            GenStmt(ifStmt.Body);
            _ir.AppendLine($"  br label %{jumpEnd}");
            _ir.AppendLine($"{jumpElse}:");
            GenStmt(ifStmt.Else);
            _ir.AppendLine($"  br label %{jumpEnd}");
            _ir.AppendLine($"{jumpEnd}:");
        }
        else
        {
            _ir.AppendLine($"  br i1 %{conditionId}, label %{jumpBody}, label %{jumpEnd}");
            _ir.AppendLine($"{jumpBody}:");
            GenStmt(ifStmt.Body);
            _ir.AppendLine($"  br label %{jumpEnd}");
            _ir.AppendLine($"{jumpEnd}:");
        }
    }

    private void GenForStmt(Stmt.ForStmt forStmt)
    {
        string forBody = $"for_body{_identifier++}";
        if (forStmt.Condition is null) //infinite loop
        {
            _ir.AppendLine($"  br label %{forBody}");
            _ir.AppendLine($"{forBody}:");
            GenStmt(forStmt.Body);
            _ir.AppendLine($"  br label %{forBody}");
            return;
        }
        
        if (forStmt.Start is not null) GenStmt(forStmt.Start);
        string forCondition = $"for_condition{_identifier++}";
        string forEnd = $"for_end{_identifier++}";
        string forIteration = $"for_iteration{_identifier++}";
        _ir.AppendLine($"  br label %{forCondition}");
        _ir.AppendLine($"{forCondition}:");
        int conditionId = GenExpr(forStmt.Condition);
        _ir.AppendLine($"  br i1 %{conditionId}, label %{forBody}, label %{forEnd}");
        _ir.AppendLine($"{forBody}:");
        GenStmt(forStmt.Body);
        if (forStmt.Iteration is not null)
        {
            _ir.AppendLine($"  br label %{forIteration}");
            _ir.AppendLine($"{forIteration}:");
            GenExpr(forStmt.Iteration);
        }
        _ir.AppendLine($"  br label %{forCondition}");
        _ir.AppendLine($"{forEnd}:");
    }

    private void GenPrintfStmt(Stmt.PrintfStmt printfStmt)
    {
        int id;
        if (_stringLiterals.TryGetValue(printfStmt.Format.Value, out id) == false)
        {
            id = _stringLiterals.Count();
            _stringLiterals.Add(printfStmt.Format.Value, id);
        }

        List<int> argIds = [];
        foreach (var arg in printfStmt.Args)
        {
            argIds.Add(GenExpr(arg));
        }
        _ir.Append($"  %{_identifier++} = call i32 (ptr, ...) @printf(ptr @.str.{id}");
        for (int i = 0; i < argIds.Count; i++)
        {
            _ir.Append($", {printfStmt.Args[i].Type.LLVMName} %{argIds[i]}");
        }
        _ir.AppendLine(")");
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

        string value = "";
        int finalId = -1; //TODO this doesn't work if we want a = b = 1
        if (e.Op is not null)
        {
            _ir.AppendLine($"  %{_identifier} = load {type.LLVMName}, ptr %{e.Name.Text}");
            _identifier++;
            value = e.Value.LiteralValue ?? $"%{finalId = GenExpr(e.Value)}";
            switch (e.Op.Type)
            {
                case TokenType.PlusEquals:
                    _ir.AppendLine($"  %{_identifier} = add nsw {e.Type.LLVMName} %{_identifier - 1}, {value}");
                    break;
                case TokenType.MinusEquals:
                    _ir.AppendLine($"  %{_identifier} = sub nsw {e.Type.LLVMName} %{_identifier - 1}, {value}");
                    break;
                case TokenType.StarEquals:
                    _ir.AppendLine($"  %{_identifier} = mul nsw {e.Type.LLVMName} %{_identifier - 1}, {value}");
                    break;
                case TokenType.SlashEquals:
                    _ir.AppendLine($"  %{_identifier} = sdiv {e.Type.LLVMName} %{_identifier - 1}, {value}");
                    break;
                default:
                    throw new NotImplementedException($"{e.Op.Type} not implemented");
            }
            _ir.AppendLine($"  store {type.LLVMName} %{_identifier}, ptr %{e.Name.Text}");
            _identifier++;
            return finalId;
        }
        
        value = e.Value.LiteralValue ?? $"%{finalId = GenExpr(e.Value)}";
        _ir.AppendLine($"  store {type.LLVMName} {value}, ptr %{e.Name.Text}");
        return finalId;
    }

    private int GenBinary(Expr.Binary e)
    {
        string leftValue = e.Left.LiteralValue ?? $"%{GenExpr(e.Left)}";
        string rightValue = e.Right.LiteralValue ?? $"%{GenExpr(e.Right)}";
        string operation;
        switch (e.Operator.Type)
        {
            case TokenType.Plus:
                operation = "add nsw";
                break;
            case TokenType.Minus:
                operation = "sub nsw";
                break;
            case TokenType.Star:
                operation = "mul nsw";
                break;
            case TokenType.Slash:
                operation = "sdiv";
                break;
            case TokenType.DoubleEquals:
                operation = "icmp eq";
                break;
            case TokenType.ExclamationEquals:
                operation = "icmp ne";
                break;
            case TokenType.Greater:
                operation = "icmp sgt";
                break;
            case TokenType.GreaterEquals:
                operation = "icmp sge";
                break;
            case TokenType.Less:
                operation = "icmp slt";
                break;
            case TokenType.LessEquals:
                operation = "icmp sle";
                break;
            case TokenType.Percent:
                operation = "srem";
                break;
            default:
                throw new NotImplementedException();
        }
        _ir.AppendLine($"  %{_identifier} = {operation} {e.Left.Type.LLVMName} {leftValue}, {rightValue}");

        return _identifier++;
    }
}