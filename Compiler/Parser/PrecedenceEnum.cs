namespace Compiler.Parser;

public enum PrecedenceEnum
{
    None = 0,

    Assignment,   // = += -= *= /=
    LogicalOr,
    LogicalAnd,
    Equality,     // == !=
    Comparison,   // < <= > >=
    Term,         // + -
    Factor,       // * / %
    Unary,        // ! -
    Call,         // () . []
    Primary,
}