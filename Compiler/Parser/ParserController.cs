using System.Text;
using Compiler.Lexer;
using Compiler.SemanticAnalysis;

namespace Compiler.Parser;

public class ParserController(List<Token> tokens)
{
    public Scope Scope { get; } = new Scope();
    private List<Stmt> _stmts = [];
    private int _current;
    public List<Stmt> Parse()
    {
        while (!IsAtEnd())
        {
            try
            {
                var declaration = Declaration();
                _stmts.Add(declaration);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Synchronize();
            }
        }

        return _stmts;
    }

    private Stmt Declaration()
    {
        MatchNewlines();
        if (Match(TokenType.Fn))
        {
            return FunctionDeclaration();
        }

        if (Match(TokenType.Struct))
        {
            return StructDeclaration();
        }
        
        if (CheckAhead(TokenType.Identifier, TokenType.ColonEquals) 
            || CheckAhead(TokenType.Identifier, TokenType.Colon))
        {
            return VarDeclaration();
        }

        return Statement();
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected var name");
        if (Match(TokenType.ColonEquals))
        {
            var expr = Expression();
            Consume(TokenType.Newline, "Expected newline at end of statement.");
            return new Stmt.VarDeclaration(name, expr);
        }
        Consume(TokenType.Colon, "Expected ':' or ':=' after variable name.");
        ExprType type = ParseType();
        Consume(TokenType.Newline, "Expected newline at end of statement.");
        //TODO support   duckCount : i32 = 5
        return new Stmt.VarDeclaration(name, null, type);
    }

    private Stmt StructDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected struct name.");
        var fields = new Dictionary<Token, ExprType>();
        Consume(TokenType.OpenCurly, "Expected { after struct name.");
        while (!Check(TokenType.CloseCurly))
        {
            MatchNewlines();
            var fieldName = Consume(TokenType.Identifier, "Expected field name.");
            Consume(TokenType.Colon, "Expected ':' after field name.");
            var fieldType = ParseType();
            fields.Add(fieldName, fieldType);
            Match(TokenType.Comma); //dangling commas are ok!
            MatchNewlines();
        }
        Consume(TokenType.CloseCurly, "Expected } at end of struct.");
        return new Stmt.StructDeclaration(name, fields);
    }

    private Stmt FunctionDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected function name.");
        Consume(TokenType.OpenParen, "Expected open paren after function name.");
        
        var args = new List<(Token name, ExprType? type)>();
        while (!Check(TokenType.CloseParen))
        {
            var argName = Consume(TokenType.Identifier, "Expected argument name.");
            Consume(TokenType.Colon, "Expected ':' after argument name.");
            var argType = ParseType();
            args.Add((argName, argType));
            Match(TokenType.Comma); //dangling commas are ok!
        }
        
        Consume(TokenType.CloseParen, "Expected close paren.");

        ExprType? returnType = null;
        if (Match(TokenType.Arrow))
        {
            var returnTypeToken = Consume(TokenType.Identifier, "Expected function return type");
            returnType = ExprType.GetType(returnTypeToken.Text);
        }
        Scope.AddFunction(name.Text, returnType);
        Consume(TokenType.OpenCurly, "Expected open brace.");

        var body = new Stmt.Block(Block());
        return new Stmt.Function(name, args, body, returnType);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.Return))
        {
            return ReturnStatement();
        }
        if (Match(TokenType.If))
        {
            return IfStatement();
        }
        if (Match(TokenType.OpenCurly))
        {
            return new Stmt.Block(Block());
        }
        if (Match(TokenType.For))
        {
            return ForStatement();
        }
        if (Match(TokenType.Printf))
        {
            return PrintfStatement();
        }
        //TODO add defer
        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        var expr = Expression();
        Consume(TokenType.Newline, "Expected newline at end of statement.");
        return new Stmt.ReturnStmt(expr);
    }

    private Stmt IfStatement()
    {
        Expr e = Expression();
        Match(TokenType.OpenCurly);
        Stmt body = new Stmt.Block(Block());
        Stmt? elseStmt = null;
        if (Match(TokenType.Else))
        {
            if (Match(TokenType.OpenCurly))
            {
                elseStmt = new Stmt.Block(Block());
            }
            else if (Match(TokenType.If))
            {
                elseStmt = IfStatement();
            }
        }

        return new Stmt.IfStmt(e, body, elseStmt);
    }

    private Stmt ForStatement()
    {
        if (Match(TokenType.OpenCurly)) // infinite loop   for { true }
        {
            return new Stmt.ForStmt(null, null, null, new Stmt.Block(Block()));
        }

        Stmt? start = null;
        List<Expr> expressions = [];
        if (CheckAhead(TokenType.Identifier, TokenType.ColonEquals))
        {
            var name = Consume(TokenType.Identifier, "Expected var name");
            Consume(TokenType.ColonEquals, "Expected ':=' after variable name.");
            var expr = Expression();
            start = new Stmt.VarDeclaration(name, expr);
        }
        else
        {
            expressions.Add(Expression());
            if (Match(TokenType.OpenCurly)) // for { condition }
            {
                return new Stmt.ForStmt(
                    null, 
                    expressions.First(), 
                    null, 
                    new Stmt.Block(Block()));
            }
        }
        
        // for { start|expr, expr, expr }
        Consume(TokenType.Comma, "Expected comma");
        expressions.Add(Expression());
        Consume(TokenType.Comma, "Expected second comma");
        expressions.Add(Expression());
        
        Consume(TokenType.OpenCurly, "Expected open curly brace");
        Stmt body = new Stmt.Block(Block());

        if (start is not null)
        {
            return new Stmt.ForStmt(start, expressions[0], expressions[1], body);
        }
        return new Stmt.ForStmt(
            new Stmt.Expression(expressions[0]),
            expressions[1],
            expressions[2],
            body);
    }

    private Stmt PrintfStatement()
    {
        List<Expr> args = [];
        Consume(TokenType.OpenParen, "Printf needs (");
        var literal = Consume(TokenType.StringLiteral, "Printf needs a format string like \"%d\\n\")");
        var formatString = new Expr.Literal.String(TrimStringLiteral(literal.Text));
        while (Match(TokenType.Comma))
        {
            args.Add(Expression());
        }
        Consume(TokenType.CloseParen, "Printf needs closing )");
        Consume(TokenType.Newline, "Expected newline at end of statement.");
        
        return new Stmt.PrintfStmt(formatString, args);
    }

    private Expr Expression()
    {
        return ParseExpression(0);
    }
    
    private Expr ParseExpression(int precedence)
    {
        var token = Advance();
        var left = ParsePrefix(token);

        while (precedence < Precedence.GetPrecedence(Peek()))
        {
            var op = Advance();
            left = ParseInfix(left, op);
        }

        return left;
    }
    
    private Expr ParsePrefix(Token token)
    {
        return token.Type switch
        {
            TokenType.IntegerLiteral =>
                new Expr.Literal.Integer(Convert.ToInt64(token.Text)),
            
            TokenType.FloatLiteral =>
                new Expr.Literal.Float(Convert.ToDouble(token.Text)),
            
            TokenType.StringLiteral =>
                new Expr.Literal.String(TrimStringLiteral(token.Text)),

            TokenType.Identifier =>
                ParseIdentifier(token),

            TokenType.Minus =>
                new Expr.Unary(token, ParseExpression((int)PrecedenceEnum.Unary)),
            
            TokenType.Exclamation =>
                new Expr.Unary(token, ParseExpression((int)PrecedenceEnum.Unary)),

            TokenType.OpenParen =>
                ParseGrouping(),

            _ => throw Error(token, "Expected expression.")
        };
    }

    private Expr ParseIdentifier(Token token)
    {
        if (Match(TokenType.OpenParen)) // function call
        {
            List<Expr> args = [];
            while (!Match(TokenType.CloseParen))
            {
                Expr arg = ParseExpression(0);
                args.Add(arg);
                Match(TokenType.Comma); // dangling commas are ok. TODO check: Does this make them optional though?
            }
            return new Expr.FunctionCall(token, args);
        }

        if (Match(TokenType.OpenSquare)) // Array index
        {
            Expr index = ParseExpression(0);
            Consume(TokenType.CloseSquare, "Expected closing ].");
            return new Expr.ArrayIndex(token, index);
        }

        return new Expr.Variable(token);
    }

    private string TrimStringLiteral(string s)
    {
        return s.Substring(1, s.Length - 2);
    }

    private Expr ParseGrouping()
    {
        var expr = ParseExpression(0);
        Consume(TokenType.CloseParen, "Expected ')' after expression.");
        return new Expr.Grouping(expr);
    }
    
    private Expr ParseInfix(Expr left, Token op)
    {
        int precedence = Precedence.GetPrecedence(op);
        var right = ParseExpression(precedence);

        return op.Type switch
        {
            TokenType.Plus or
            TokenType.Minus or
            TokenType.Star or
            TokenType.Slash or
            TokenType.Percent or
            TokenType.Greater or 
            TokenType.GreaterEquals or  
            TokenType.Less or
            TokenType.LessEquals or 
            TokenType.DoubleEquals or 
            TokenType.Or or
            TokenType.And or
            TokenType.ExclamationEquals =>
                new Expr.Binary(left, op, right),

            TokenType.Equals or
            TokenType.PlusEquals or 
            TokenType.MinusEquals or 
            TokenType.StarEquals or 
            TokenType.SlashEquals or 
            TokenType.PercentEquals =>
                new Expr.Assign(left, right, op),
            
            _ => throw Error(op, "Unknown operator.")
        };
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Newline, "Expected newline at end of statement.");
        return new Stmt.Expression(expr);
    }

    private List<Stmt> Block()
    {
        MatchNewlines();
        List<Stmt> statements = [];
        while (!Check(TokenType.CloseCurly) && !IsAtEnd())
        {
            MatchNewlines();
            statements.Add(Declaration());
            MatchNewlines();
        }

        Consume(TokenType.CloseCurly, "Expected '}' after block.");
        MatchNewlines();
        return statements;
    }

    private ExprType ParseType()
    {
        int arraySize = -1;
        if (Match(TokenType.OpenSquare))
        {
            if (Check(TokenType.IntegerLiteral))
            {
                Token arraySizeToken = Consume(TokenType.IntegerLiteral, "Expected array size in [].");
                arraySize = Convert.ToInt32(arraySizeToken.Text);
            }
            else
            {
                arraySize = 0;
            }
            Consume(TokenType.CloseSquare, "Expected closing ].");
        }

        Token typeToken = Consume(TokenType.Identifier, "Expected type.");
        ExprType? type = ExprType.GetType(typeToken.Text);
        if (type is null) throw Error(typeToken, $"Could not parse type {typeToken.Text}.");
        if (arraySize > -1)
        {
            return new ExprType(type.LLVMName, arraySize);
        }
        return type;
    }

    private void MatchNewlines()
    {
        while (Match(TokenType.Newline))
        {
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var stmt in _stmts)
        {
            sb.Append(Printer.Print(stmt));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private bool IsAtEnd()
    {
        return _current >= tokens.Count;
    }
    
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }
    
    private bool CheckAhead(params TokenType[] types)
    {
        for (int i = 0; i < types.Length; i++)
        {
            int index = _current + i;
            if (index >= tokens.Count)
                return false;

            if (tokens[index].Type != types[i])
                return false;
        }
        return true;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Token Peek()
    {
        return tokens[_current];
    }

    private Token Previous()
    {
        return tokens[_current - 1];
    }

    private Exception Error(Token token, string message)
    {
        return new Exception($"[Line {token.Line}:{token.Column}] Error at '{token.Text}': {message}");
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Newline) return;

            switch (Peek().Type)
            {
                case TokenType.Fn:
                case TokenType.For:
                case TokenType.If:
                case TokenType.Printf:
                case TokenType.Return:    
                    return;
                default:
                    Advance();
                    break;
            }
        }
    }
}