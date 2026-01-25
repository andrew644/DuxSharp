using DuxSharp.Lexer;

namespace DuxSharp;

public class DuxSharp
{
    static void Main(String[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: DuxSharp <path>");
            return;
        }
        Console.WriteLine("Compiling: " + args[0]);
        
        string text = File.ReadAllText(args[0]);
        LexerController lexerController = new LexerController();
        var tokens = lexerController.Lex(text);
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }
}