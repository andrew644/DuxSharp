using DuxSharp.Extension;

namespace DuxSharp.Lexer;

public class LexerController
{
    private readonly List<Token> _tokens = [];
    
    private int _line;
    private int _column;
    private int _index;
    private int _tokenStart;
    private int _tokenStartColumn;
    private string _file = "";
    
    public List<Token> Lex(string file)
    {
        this._file = file;
        while (_index < file.Length)
        {
            _tokenStart = _index;
            _tokenStartColumn = _column;
            char c = file[_index];
            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    _line++;
                    _column = -1; //gets set to 0 in Advance
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

        return _tokens;
    }

    private void AddToken(TokenType t)
    {
        _tokens.Add(
            new Token(
                _file.Substring(_tokenStart, _index - _tokenStart + 1),
            _line,
            _tokenStartColumn,
            t));
    }

    private char Peek(int offset = 1)
    {
        if (offset + _index >= _file.Length)
        {
            return '\0';
        }
        
        return _file[offset + _index];
    }
    
    private void Advance()
    {
        _column++;
        _index++;
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
            if (Peek() == '\n') _line++;
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
        string text = _file.Substring(_tokenStart, _index - _tokenStart);
        TokenType type = Keyword.Keywords.GetOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }
}