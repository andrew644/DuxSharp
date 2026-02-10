namespace DuxSharp.Lexer;

public enum TokenType
{
    // Math
    Plus,
    Minus,
    Star,
    Slash,
    Equals,
    DoubleEquals,
    GreaterEquals,
    LessEquals,
    Less,
    Greater,
    ExclamationEquals,
    Percent,
    PlusEquals,
    MinusEquals,
    StarEquals,
    SlashEquals,
    PercentEquals,
    
    // Groups
    OpenParen,
    CloseParen,
    OpenCurly,
    CloseCurly,
    OpenSquare,
    CloseSquare,
    
    // Delimiters
    Colon,
    Dot,
    Arrow,
    Comma,
    Bar,
    Newline,
    
    // Literals
    StringLiteral,
    FloatLiteral,
    IntegerLiteral,
    
    // Variable
    Identifier,
    ColonEquals,
    
    // Logic
    And,
    Or,
    True,
    False,
    Exclamation,
    
    // Control
    If,
    Else,
    For,
    Break,
    Continue,
    Goto,
    
    // Function
    Fn,
    Return,
    Defer,
    
    // Special Types
    Enum,
    Struct,
    Union,
    
    // Package
    Import,
    
    // To be removed
    Printf,
}