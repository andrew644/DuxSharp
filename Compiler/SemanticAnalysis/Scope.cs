using Compiler.Lexer;
using Compiler.Parser;

namespace Compiler.SemanticAnalysis;

public class Scope
{
    private readonly Dictionary<string, ExprType> _environmentVar = new Dictionary<string, ExprType>();
    private readonly Dictionary<string, ExprType> _environmentFunc = new Dictionary<string, ExprType>();
    private readonly Dictionary<string, Dictionary<string, StructField>> _environmentStruct = new Dictionary<string, Dictionary<string, StructField>>();

    public void AddVar(string name, ExprType type)
    {
        _environmentVar[name] = type;
    }

    public ExprType? GetVar(string name)
    {
        if (_environmentVar.TryGetValue(name, out ExprType result))
        {
            return result;
        }

        return null;
    }
    
    public void AddFunction(string name, ExprType type)
    {
        _environmentFunc[name] = type;
    }

    public ExprType? GetFunction(string name)
    {
        if (_environmentFunc.TryGetValue(name, out ExprType result))
        {
            return result;
        }

        return null;
    }

    public void AddStruct(string name, Dictionary<string, StructField> fields)
    {
        _environmentStruct[name] = fields;
    }

    public StructField? GetStructField(string structName, string fieldName)
    {
        if (_environmentStruct.TryGetValue(structName, out Dictionary<string, StructField> fields))
        {
            if (fields.TryGetValue(fieldName, out StructField result))
            {
                return result;
            }
        }

        return null;
    }

    public void ResetVars()
    {
        _environmentVar.Clear();
    }
}