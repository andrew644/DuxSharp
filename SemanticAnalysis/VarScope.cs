using DuxSharp.Parser;

namespace DuxSharp.SemanticAnalysis;

public class VarScope
{
    private Dictionary<string, ExprType> _environment = new Dictionary<string, ExprType>();

    public void AddVar(string name, ExprType type)
    {
        _environment[name] = type;
    }

    public ExprType? GetVar(string name)
    {
        if (_environment.TryGetValue(name, out ExprType result))
        {
            return result;
        }

        return null;
    }
}