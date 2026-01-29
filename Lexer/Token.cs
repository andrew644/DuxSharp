namespace DuxSharp.Lexer;

public record Token(string Text, int Line, int Column, TokenType Type)
{
    public override string ToString()
    {
        return Type + " " + Line + ":" + Column + " - " + Text;
    }
}