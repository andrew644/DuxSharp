namespace DuxSharp.Lexer;

public class Token
{
    public Token(string text, int line, int column, TokenType type)
    {
        Text = text;
        Line = line;
        Column = column;
        Type = type;
    }
    
    public readonly string Text;
    public readonly int Line;
    public readonly int Column;
    public readonly TokenType Type;

    public override string ToString()
    {
        return Type + " " + Line + ":" + Column + " - " + Text;
    }
}