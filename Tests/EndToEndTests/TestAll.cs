using System.Diagnostics;
using JetBrains.Annotations;

namespace Tests.EndToEndTests;

[TestSubject(typeof(Compiler.Entry))]
public class TestAll
{
    static (int ExitCode, string StdOut, string StdErr) RunProcess(string file, string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var p = Process.Start(psi)!;
        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();

        return (p.ExitCode, stdout, stderr);
    }
    
    [Theory]
    [InlineData("function")]
    public void RunTests(string name)
    {
        var source = $"../../../../dux_test_src/{name}.dux";
        var expected = File.ReadAllText($"../../../../dux_test_src/{name}.expected");
    }
}