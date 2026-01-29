using System.Text;
using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public class ParserController(List<Token> tokens)
{
    private List<Stmt> _stmts = [];
    private int _current;
    public List<Stmt> Parse()
    {
        while (!IsAtEnd())
        {
            _stmts.Add(Declaration());
        }

        return _stmts;
    }

    private Stmt Declaration()
    {
        if (Match(TokenType.Fn))
        {
            return FunctionDeclaration();
        }
        
        if (CheckAhead(TokenType.Identifier, TokenType.Equals))
        {
            return VarDeclaration();
        }

        return Statement();
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected var name");
        Consume(TokenType.Equals, "Expected '=' after variable name.");
        return new Stmt.VarDeclaration(name, Expression());
    }

    private Stmt FunctionDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected function name.");
        Consume(TokenType.OpenParen, "Expected open paren after function name.");
        
        var args = new List<Token>();
        if (!Check(TokenType.CloseParen))
        {
            //TODO args
        }
        
        Consume(TokenType.CloseParen, "Expected close paren.");
        //TODO return type
        Consume(TokenType.OpenBrace, "Expected open brace.");

        var body = Block();
        return new Stmt.Function(name, args, body, null);
    }

    private Stmt Statement()
    {
        return ExpressionStatement();
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
            TokenType.Number =>
                new Expr.Literal(token.Text),

            TokenType.Identifier =>
                new Expr.Variable(token),

            //TokenType.Minus =>
            //    new Expr.Unary(token, ParseExpression(Precedence.Unary)),

            TokenType.OpenParen =>
                ParseGrouping(),

            _ => throw Error(token, "Expected expression.")
        };
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
            TokenType.Slash =>
                new Expr.Binary(left, op, right),

            TokenType.Equals =>
                left is Expr.Variable v
                    ? new Expr.Assign(v.Name, right)
                    : throw Error(op, "Invalid assignment target."),

            _ => throw Error(op, "Unknown operator.")
        };
    }

    private Stmt ExpressionStatement()
    {
        return new Stmt.Expression(Expression());
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = [];
        while (!Check(TokenType.CloseBrace) && !IsAtEnd())
        {
            statements.Add(Declaration()); //TODO I think this should just be statements
        }

        Consume(TokenType.CloseBrace, "Expected '}' after block.");
        return statements;
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
}