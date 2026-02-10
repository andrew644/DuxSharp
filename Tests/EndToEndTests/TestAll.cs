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
    [InlineData("function", 0)]
    [InlineData("if", 0)]
    [InlineData("fizzbuzz", 40)] //TODO void main returns 40. Should we force this to be 0?
    [InlineData("simple1", 42)]
    [InlineData("simple2", 43)]
    public void RunTests(string name, int expectedExitCode)
    {
        const string baseDir = "../../../../";
        const string testDir = $"{baseDir}dux_test_src/";
        const string outDir = $"{testDir}out/";
        var irPath = $"{outDir}{name}.ll";
        var binPath = $"{outDir}{name}";
        var srcPath = $"{testDir}{name}.dux";
        
        RunProcess("mkdir", $"-p {outDir}"); // make sure out directory exists

        var (exitCode, _, stdErr) = RunProcess($"{baseDir}Compiler/bin/Debug/net10.0/Compiler", $"{srcPath} {irPath}");
        Assert.Equal("", stdErr);
        Assert.Equal(0, exitCode);
        
        var (clangExitCode, clangStdOut, clangStdErr) = RunProcess($"clang", $"-o {binPath} {irPath}");
        Assert.Equal("", clangStdErr);
        Assert.Equal("", clangStdOut);
        Assert.Equal(0, clangExitCode);
        
        var (binExitCode, binStdOut, binStdErr) = RunProcess($"{binPath}", "");
        Assert.Equal("", binStdErr);
        
        var expected = File.ReadAllText($"{testDir}{name}.exp");
        Assert.Equal(expected, binStdOut);
        Assert.Equal(expectedExitCode, binExitCode);
    }
}