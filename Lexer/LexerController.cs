using System.Diagnostics.Tracing;
using DuxSharp.Extension;

namespace DuxSharp.Lexer;

public class LexerController
{
    private List<Token> tokens = [];
    
    private int line = 0;
    private int column = 0;
    private int index = 0;
    private int tokenStart = 0;
    private string file;
    
    public List<Token> Lex(string file)
    {
        this.file = file;
        while (index < file.Length)
        {
            tokenStart = index;
            char c = file[index];
            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    line++;
                    column = -1; //gets set to 0 in Advance
                    break;
                case '(':
                    AddToken(TokenType.OpenParen);
                    break;
                case ')':
                    AddToken(TokenType.CloseParen);
                    break;
                case '{':
                    AddToken(TokenType.OpenBrace);
                    break;
                case '}':
                    AddToken(TokenType.CloseBrace);
                    break;
                case '+':
                    AddToken(TokenType.Plus);
                    break;
                case '/':
                    if (Peek() == '/') // Single line comments
                    {
                        while (Peek() != '\n' && Peek() != '\0') Advance();
                    }
                    break;
                case '=':
                    LexDoubleChar('=', TokenType.Equals, TokenType.DoubleEquals);
                    break;
                case '"':
                    LexString();
                    break;
                
                default:
                    if (IsDigit(c))
                    {
                        LexNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        LexIdentifier();
                    }
                    else
                    {
                        //TODO error
                    }
                    break;
            }
            Advance();
        }

        return tokens;
    }

    private void AddToken(TokenType t)
    {
        tokens.Add(
            new Token(
                file.Substring(tokenStart, index - tokenStart + 1),
            line,
            tokenStart,
            t));
    }

    private char Peek(int offset = 1)
    {
        if (offset + index >= file.Length)
        {
            return '\0';
        }
        
        return file[offset + index];
    }
    
    private void Advance()
    {
        column++;
        index++;
    }

    private void LexDoubleChar(char secondChar, TokenType singleCharToken, TokenType twoCharToken)
    {
        if (Peek() == secondChar)
        {
            Advance();
            AddToken(twoCharToken);
        }
        else
        {
            AddToken(singleCharToken);
        }
    }

    private void LexString()
    {
        while (Peek() != '"' && Peek() != '\0')
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (Peek() == '\0')
        {
            //TODO error
            return;
        }
        
        Advance(); //closing "
        AddToken(TokenType.String);
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private bool IsAlpha(char c)
    {
        return c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z' or '_');
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private void LexNumber()
    {
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(Peek(2)))
        {
            Advance(); // Consume the "."
            while (IsDigit(Peek())) Advance();
        } 
        
        AddToken(TokenType.Number);
    }

    private void LexIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        string text = file.Substring(tokenStart, index - tokenStart);
        TokenType type = Keyword.Keywords.GetOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }
}