using DuxSharp.Extension;

namespace DuxSharp.Lexer;

public class LexerController(string file)
{
    private readonly List<Token> _tokens = [];
    
    private int _line = 1;
    private int _column;
    private int _index;
    private int _tokenStart;
    private int _tokenStartColumn;
    
    public List<Token> Lex()
    {
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
                    AddToken(TokenType.Newline);
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
                case '[':
                    AddToken(TokenType.OpenBracket);
                    break;
                case ']':
                    AddToken(TokenType.CloseBracket);
                    break;
                case '+':
                    LexDoubleChar(TokenType.Plus, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.PlusEquals},
                    });
                    break;
                case '-':
                    LexDoubleChar(TokenType.Minus, new Dictionary<char, TokenType>
                    {
                        {'>', TokenType.Arrow},
                        {'=', TokenType.MinusEquals},
                    });
                    break;
                case '*':
                    LexDoubleChar(TokenType.Star, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.StarEquals},
                    });
                    break;
                case '/':
                    if (Peek() == '/') // Single line comments
                    {
                        while (Peek() != '\n' && Peek() != '\0') Advance();
                        break;
                    }

                    if (Peek() == '*') // Multiline comment
                    {
                        Advance();
                        while ((Peek() != '*' || Peek(2) != '/') && Peek() != '\0' && Peek(2) != '\0') Advance();
                        Advance();
                        Advance();
                        break;
                    }
                    
                    LexDoubleChar(TokenType.Slash, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.SlashEquals},
                    });
                    break;
                case '=':
                    LexDoubleChar(TokenType.Equals, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.DoubleEquals},
                    });
                    break;
                case '<':
                    LexDoubleChar(TokenType.Less, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.LessEquals},
                    });
                    break;
                case '>':
                    LexDoubleChar(TokenType.Greater, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.GreaterEquals},
                    });
                    break;
                case '!':
                    LexDoubleChar(TokenType.Exclamation, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.ExclamationEquals},
                    });
                    break;
                case ':':
                    LexDoubleChar(TokenType.Colon, new Dictionary<char, TokenType>
                    {
                        {'=', TokenType.ColonEquals},
                    });
                    break;
                case '|':
                    AddToken(TokenType.Bar);
                    break;
                case '.':
                    AddToken(TokenType.Dot);
                    break;
                case ',':
                    AddToken(TokenType.Comma);
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
                file.Substring(_tokenStart, _index - _tokenStart + 1),
            _line,
            _tokenStartColumn,
            t));
    }

    private char Peek(int offset = 1)
    {
        if (offset + _index >= file.Length)
        {
            return '\0';
        }
        
        return file[offset + _index];
    }
    
    private void Advance()
    {
        _column++;
        _index++;
    }

    private void LexDoubleChar(TokenType singleCharToken, Dictionary<char, TokenType> secondCharMapping)
    {
        char next = Peek();
        bool exists = secondCharMapping.TryGetValue(next, out var twoCharToken);
        if (exists)
        {
            Advance();
            AddToken(twoCharToken);
            return;
        }
        
        AddToken(singleCharToken);
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
        AddToken(TokenType.StringLiteral);
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
        bool isFloat = false;
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(Peek(2)))
        {
            isFloat = true;
            Advance(); // Consume the "."
            while (IsDigit(Peek())) Advance();
        } 
        
        AddToken(isFloat ? TokenType.FloatLiteral : TokenType.IntegerLiteral);
    }

    private void LexIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        string text = file.Substring(_tokenStart, _index - _tokenStart + 1);
        TokenType type = Keyword.Keywords.GetOrDefault(text, TokenType.Identifier);
        AddToken(type);
    }
}