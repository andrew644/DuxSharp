namespace DuxSharp.Parser;

public class Type
{
    public Type(string typeName)
    {
        this.TypeName = typeName;
    }

    public string TypeName { get; private set; }
}