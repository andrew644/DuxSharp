using Compiler.Lexer;

namespace Compiler.Parser;

public class ExprType
{
    public static ExprType Tu8 { get; } = new ExprType("i8", true);
    public static ExprType Ti32 { get; } = new ExprType("i32");
    public static ExprType Ti64 { get; } = new ExprType("i64");
    public static ExprType Tf32 { get; } = new ExprType("float");
    public static ExprType Tf64 { get; } = new ExprType("double");
    public static ExprType Tbool { get; } = new ExprType("i1");
    public static ExprType Tstring { get; } = new ExprType("string");
    
    private static Dictionary<string, ExprType> _typeLookup = new Dictionary<string, ExprType>()
    {
        { "u8", Tu8 },
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
        LLVMName = llvmName;
    }

    public ExprType(string llvmName, int arraySize)
    {
        LLVMName = llvmName;
        ArraySize = arraySize;
    }

    public ExprType(string llvmName, bool unsignedInt)
    {
        LLVMName = llvmName;
        UnsignedInt = unsignedInt;
    }

    public string LLVMName { get; private set; }
    public int ArraySize { get; private set; } = -1;
    public bool UnsignedInt { get; private set; } = false;

    public override string ToString()
    {
        return LLVMName;
    }
}