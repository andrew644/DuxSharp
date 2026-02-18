using System.Text;
using Compiler.Lexer;
using Compiler.Parser;
using Compiler.SemanticAnalysis;

namespace Compiler.CodeGeneration;

public class CodeGen(List<Stmt> ast)
{
    private readonly StringBuilder _ir = new StringBuilder();
    private readonly StringBuilder _irHeader = new StringBuilder();
    private readonly Dictionary<string, int> _stringLiterals = new Dictionary<string, int>();
    private int _identifier = 1;
    private Scope _scope = new Scope();

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
        _scope = new Scope();
        
        _ir.Append("define ");
        _ir.Append(function.ReturnType is not null ? function.ReturnType.LLVMName : "void");
        _ir.Append($" @{function.Name.Text}(");
        
        // Args
        bool first = true;
        StringBuilder argsAlloca = new StringBuilder();
        _identifier--; // function args start at %0 not %1
        foreach (var arg in function.Args)
        {
            if (!first) _ir.Append(", ");
            _ir.Append($"{arg.type.LLVMName} %id.{_identifier}");
            argsAlloca.AppendLine($"  %{arg.name.Text} = alloca {arg.type.LLVMName}");
            argsAlloca.AppendLine($"  store {arg.type.LLVMName} %id.{_identifier}, ptr %{arg.name.Text}");
            _scope.AddVar(arg.name.Text, arg.type);
            _identifier++;
            first = false;
        }

        _identifier++; // function args start at %0 not %1, now we go back up
        
        // Body
        _ir.AppendLine(") {");
        _ir.AppendLine(argsAlloca.ToString());
        foreach (var stmt in function.Body)
        {
            GenStmt(stmt);
        }

        if (function.ReturnType is null)
        {
            _ir.AppendLine("  ret void");
        }
        _ir.AppendLine("  unreachable");
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
        string value = r.Expr.LiteralValue ?? $"%id.{GenExpr(r.Expr)}";
        _ir.AppendLine($"  ret {r.Expr.Type.LLVMName} {value}");
        _ir.AppendLine($"hidden_basic_block.{_identifier++}:");
        _ir.AppendLine("  unreachable");
    }

    private void GenVarDeclaration(Stmt.VarDeclaration vd)
    {
        ExprType type = vd.Type ?? vd.Value.Type;
        _scope.AddVar(vd.Name.Text, type);
        string llvmName = type.LLVMName;
        if (type.ArraySize > 0)
        {
            llvmName = $"[{type.ArraySize} x {type.LLVMName}]";
        }
        _ir.Append($"  %{vd.Name.Text} = alloca {llvmName}\n");
        
        if (vd.Value is null) return;
        string value = vd.Value.LiteralValue ?? $"%id.{GenExpr(vd.Value)}";
        _ir.AppendLine($"  store {vd.Value.Type.LLVMName} {value}, ptr %{vd.Name.Text}");
    }

    private void GenIfStmt(Stmt.IfStmt ifStmt)
    {
        int conditionId = GenExpr(ifStmt.Condition);
        string jumpBody = $"if_body.{_identifier++}";
        string jumpEnd = $"if_end.{_identifier++}";
        if (ifStmt.Else is not null)
        {
            string jumpElse = $"else_body.{_identifier++}";
            _ir.AppendLine($"  br i1 %id.{conditionId}, label %{jumpBody}, label %{jumpElse}");
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
            _ir.AppendLine($"  br i1 %id.{conditionId}, label %{jumpBody}, label %{jumpEnd}");
            _ir.AppendLine($"{jumpBody}:");
            GenStmt(ifStmt.Body);
            _ir.AppendLine($"  br label %{jumpEnd}");
            _ir.AppendLine($"{jumpEnd}:");
        }
    }

    private void GenForStmt(Stmt.ForStmt forStmt)
    {
        string forBody = $"for_body.{_identifier++}";
        if (forStmt.Condition is null) //infinite loop
        {
            _ir.AppendLine($"  br label %{forBody}");
            _ir.AppendLine($"{forBody}:");
            GenStmt(forStmt.Body);
            _ir.AppendLine($"  br label %{forBody}");
            return;
        }
        
        if (forStmt.Start is not null) GenStmt(forStmt.Start);
        string forCondition = $"for_condition.{_identifier++}";
        string forEnd = $"for_end.{_identifier++}";
        string forIteration = $"for_iteration.{_identifier++}";
        _ir.AppendLine($"  br label %{forCondition}");
        _ir.AppendLine($"{forCondition}:");
        int conditionId = GenExpr(forStmt.Condition);
        _ir.AppendLine($"  br i1 %id.{conditionId}, label %{forBody}, label %{forEnd}");
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

        List<string> argCode = [];
        foreach (var arg in printfStmt.Args)
        {
            argCode.Add(arg.LiteralValue ?? $"%id.{GenExpr(arg)}");
        }
        _ir.Append($"  %id.{_identifier++} = call i32 (ptr, ...) @printf(ptr @.str.{id}");
        for (int i = 0; i < argCode.Count; i++)
        {
            _ir.Append($", {printfStmt.Args[i].Type.LLVMName} {argCode[i]}");
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
            case Expr.Unary e:
                return GenUnary(e);
            case Expr.Grouping e:
                return GenGrouping(e);
            case Expr.FunctionCall e:
                return GenFunctionCall(e);
            case Expr.ArrayIndex e:
                return GenArrayIndexRValue(e);
            default:
                throw new NotImplementedException(expr.GetType().Name);
        }
    }
    private int GenVariable(Expr.Variable v)
    {
        ExprType? type = _scope.GetVar(v.Name.Text);
        if (type is null)
        {
            throw new Exception($"Variable {v.Name.Text} not found");
        }

        _ir.AppendLine($"  %id.{_identifier} = load {type.LLVMName}, ptr %{v.Name.Text}");
        return _identifier++;
    }

    private int GenAssign(Expr.Assign e)
    {
        Expr RValue = e.Value;
        switch (e.Op.Type)
        {
            case TokenType.PlusEquals:
                RValue = new Expr.Binary(e.LValue, new Token("+", -1, -1, TokenType.Plus), e.Value);
                break;
            case TokenType.MinusEquals:
                RValue = new Expr.Binary(e.LValue, new Token("-", -1, -1, TokenType.Minus), e.Value);
                break;
            case TokenType.StarEquals:
                RValue = new Expr.Binary(e.LValue, new Token("*", -1, -1, TokenType.Star), e.Value);
                break;
            case TokenType.SlashEquals:
                RValue = new Expr.Binary(e.LValue, new Token("/", -1, -1, TokenType.SlashEquals), e.Value);
                break;
            case TokenType.Equals:
                break;
            default:
                throw new Exception($"Unsupported token type {e.Op.Type}");
        }

        return GenAssignEquals(new Expr.Assign(e.LValue, RValue, e.Op)
        {
            Type = e.Type
        });
    }

    private int GenAssignEquals(Expr.Assign e)
    {
        string lValueName;
        ExprType? type;
        switch (e.LValue)
        {
            case Expr.Variable lValue:
                lValueName = lValue.Name.Text;
                type = _scope.GetVar(lValueName);
                break;
            case Expr.ArrayIndex lValue:
                lValueName = $"id.{GenArrayIndex(lValue)}";
                type = _scope.GetVar(lValue.Name.Text);
                break;
            default:
                throw new Exception($"Unsupported lvalue {e.LValue.Type}");
        }
        
        if (type is null) throw new Exception($"var {lValueName} not found");

        int finalId = -1; //TODO this doesn't work if we want a = b = 1 // I think we can return lvalue's name in this case
        
        string value = e.Value.LiteralValue ?? $"%id.{finalId = GenExpr(e.Value)}";
        _ir.AppendLine($"  store {type.LLVMName} {value}, ptr %{lValueName}");
        
        return finalId;
    }

    private int GenBinary(Expr.Binary e)
    {
        string leftValue = e.Left.LiteralValue ?? $"%id.{GenExpr(e.Left)}";
        string rightValue = e.Right.LiteralValue ?? $"%id.{GenExpr(e.Right)}";
        string operation;
        switch (e.Op.Type)
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
            case TokenType.And:
                operation = "and";
                break;
            case TokenType.Or:
                operation = "or";
                break;
            default:
                throw new NotImplementedException($"{e.Op.Type} not implemented.");
        }
        _ir.AppendLine($"  %id.{_identifier} = {operation} {e.Left.Type.LLVMName} {leftValue}, {rightValue}");

        return _identifier++;
    }

    private int GenUnary(Expr.Unary e)
    {
        string rightValue = e.Right.LiteralValue ?? $"%id.{GenExpr(e.Right)}";
        string operation;
        switch (e.Op.Type)
        {
            case TokenType.Minus:
                operation = "sub nsw";
                break;
            default:
                throw new NotImplementedException();
        }
        
        _ir.AppendLine($"  %id.{_identifier} = {operation} {e.Right.Type.LLVMName} 0, {rightValue}");

        return _identifier++;
    }

    private int GenGrouping(Expr.Grouping e)
    {
        return GenExpr(e.Expression);
    }

    private int GenFunctionCall(Expr.FunctionCall f)
    {
        List<string> argCode = [];
        foreach (var arg in f.Args)
        {
            argCode.Add(arg.LiteralValue ?? $"%id.{GenExpr(arg)}");
        }

        _ir.Append("  ");
        string returnType = "void";
        int returnId = -1;
        if (f.Type is not null)
        {
            returnType = f.Type.LLVMName;
            _ir.Append($"%id.{_identifier} = "); // If there is a return, we save the value
            returnId = _identifier++;
        }
        _ir.Append($"call {returnType} @{f.Name.Text}(");
        for (int i = 0; i < argCode.Count; i++)
        {
            if (i > 0)
            {
                _ir.Append(", ");
            }
            _ir.Append($"{f.Args[i].Type.LLVMName} {argCode[i]}");
        }
        _ir.AppendLine(")");
        
        return returnId;
    }

    private int GenArrayIndex(Expr.ArrayIndex e)
    {
        string indexId = e.Index.LiteralValue ?? $"%id.{GenExpr(e.Index)}";
        _ir.AppendLine($"  %id.{_identifier} = sext {e.Index.Type.LLVMName} {indexId} to i64");
        indexId = $"%id.{_identifier}";
        _identifier++;
        _ir.AppendLine($"  %id.{_identifier} = getelementptr inbounds [{e.Type.ArraySize} x {e.Type.LLVMName}], ptr %{e.Name.Text}, i64 0, i64 {indexId}");
        return _identifier++;
    }

    private int GenArrayIndexRValue(Expr.ArrayIndex e)
    {
        int id = GenArrayIndex(e);
        _ir.AppendLine($"  %id.{_identifier} = load {e.Type.LLVMName}, ptr %id.{id}");
        return _identifier++;
    }
}