using DuxSharp.Lexer;

namespace DuxSharp;

public static class DuxSharp
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: DuxSharp <path>");
            return;
        }
        Console.WriteLine("Compiling: " + args[0]);
        
        string text = File.ReadAllText(args[0]);
        var lexerController = new LexerController();
        List<Token> tokens = lexerController.Lex(text);
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }
}