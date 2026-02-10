using Compiler.Parser;

namespace Compiler.SemanticAnalysis;

public class Scope
{
    private readonly Dictionary<string, ExprType> _environmentVar = new Dictionary<string, ExprType>();
    private readonly Dictionary<string, ExprType> _environmentFunc = new Dictionary<string, ExprType>();

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

    public void ResetVars()
    {
        _environmentVar.Clear();
    }
}