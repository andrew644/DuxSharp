using DuxSharp.Lexer;

namespace DuxSharp.Parser;

public class ExprType
{
    public static ExprType Ti32 { get; } = new ExprType("i32");
    public static ExprType Ti64 { get; } = new ExprType("i64");
    public static ExprType Tf32 { get; } = new ExprType("float");
    public static ExprType Tf64 { get; } = new ExprType("double");
    public static ExprType Tbool { get; } = new ExprType("i1");
    public static ExprType Tstring { get; } = new ExprType("string");
    
    private static Dictionary<string, ExprType> _typeLookup = new Dictionary<string, ExprType>()
    {
        { "i32", Ti32 },
        { "i64", Ti64 },
        { "f32", Tf32 },
        { "f64", Tf64 },
        { "bool", Tbool },
        { "string", Tstring },
    };

    public static ExprType? GetType(string typeName)
    {
        return _typeLookup.GetValueOrDefault(typeName);
    }
    
    public ExprType(string llvmName)
    {
        this.LLVMName = llvmName;
    }

    public string LLVMName { get; private set; }

    public override string ToString()
    {
        return LLVMName;
    }
}