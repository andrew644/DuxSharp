namespace DuxSharp.Lexer;

public enum TokenType
{
    Plus,
    Minus,
    Star,
    Slash,
    OpenParen,
    CloseParen,
    OpenBrace,
    CloseBrace,
    Equals,
    DoubleEquals,
    String,
    Number,
    Identifier,
    And,
    Or,
    If,
    Else,
    For,
    Fn,
    Return,
    True,
    False,
    Defer,
    Newline,
}