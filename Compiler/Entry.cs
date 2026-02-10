using Compiler.CodeGeneration;
using Compiler.Lexer;
using Compiler.Parser;
using Compiler.SemanticAnalysis;

namespace Compiler;

public static class Entry
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: DuxSharp <path> <output-path>");
            return;
        }
        Console.WriteLine($"Compiling: {args[0]} -> {args[1]}");
        
        Console.WriteLine("\nTokens:");
        string text = File.ReadAllText(args[0]);
        var lexerController = new LexerController(text);
        List<Token> tokens = lexerController.Lex();
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
        
        Console.WriteLine("\nParsing:");
        var parser = new ParserController(tokens);
        List<Stmt> ast = parser.Parse();
        Console.WriteLine(parser);
        
        Console.WriteLine("\nAnalyzing:");
        var analyzer = new SemanticAnalyzer(ast);
        analyzer.Analize(parser.Scope);
        Console.WriteLine(parser);
        
        Console.WriteLine("\nCodegen...");
        var codegen = new CodeGen(ast);
        var ir = codegen.Generate();
        File.WriteAllText(args[1], ir);
        Console.WriteLine($"Generated:\n{ir}");
    }
}