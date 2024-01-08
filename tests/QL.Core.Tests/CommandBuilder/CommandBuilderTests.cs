namespace QL.Core.Tests.CommandBuilder;

[TestFixture]
public class CommandBuilderTests
{
    [Test]
    public void Test_AddCommand_SingleCommand()
    {
        var builder = new Core.CommandBuilder();
        builder.AddCommand("echo 'Hello World'");
        var result = builder.Build();

        Assert.That(result, Is.EqualTo("echo 'Hello World'"));
    }

    [Test]
    public void Test_AddConcatenatedCommand_MultipleCommands()
    {
        var builder = new Core.CommandBuilder();
        builder.AddConcatenatedCommand("echo 'Start'")
            .AddConcatenatedCommand("ls -l")
            .AddConcatenatedCommand("echo 'End'");

        var result = builder.Build();

        Assert.That(result, Is.EqualTo("echo 'Start' && ls -l && echo 'End'"));
    }

    [Test]
    public void Test_ForLoop_Construct()
    {
        var builder = new Core.CommandBuilder();
        builder.ForLoop("i", "1 2 3")
            .AddCommand("echo $i")
            .EndFor();

        var result = builder.Build();

        Assert.That(result, Is.EqualTo("for i in 1 2 3; do\necho $i;\ndone\n"));
    }

    [Test]
    public void Test_IfElse_Construct()
    {
        var builder = new Core.CommandBuilder();
        builder.If("[ $a -eq $b ]")
            .AddCommand("echo 'Equal'")
            .Else()
            .AddCommand("echo 'Not Equal'")
            .EndIf();

        var result = builder.Build();

        Assert.That(result, Is.EqualTo("if [ $a -eq $b ]; then\necho 'Equal';\nelse\necho 'Not Equal';\nfi;\n"));
    }

    [Test]
    public void Test_AddArgument()
    {
        var builder = new Core.CommandBuilder();
        builder.AddCommand("echo").AddArgument("'ArgumentWithSpace'");

        var result = builder.Build();

        Assert.That(result, Is.EqualTo("echo 'ArgumentWithSpace'"));
    }
    
    [Test]
    public void Test_IfElifElse_Construct()
    {
        var builder = new Core.CommandBuilder();
        builder.If("[ $a -eq $b ]")
            .AddCommand("echo 'Equal'")
            .Elif("[ $a -gt $b ]")
            .AddCommand("echo 'Greater'")
            .Else()
            .AddCommand("echo 'Not Equal'")
            .EndIf();

        var result = builder.Build();

        Assert.That(result, Is.EqualTo("if [ $a -eq $b ]; then\necho 'Equal';\nelif [ $a -gt $b ]; then\necho 'Greater';\nelse\necho 'Not Equal';\nfi;\n"));
    }
    
    [Test]
    public void Test_ForLoop_WithConcatenatedCommands()
    {
        var builder = new Core.CommandBuilder();
        builder.ForLoop("i", "1 2 3")
            .AddConcatenatedCommand("echo $i")
            .AddConcatenatedCommand("echo 'Loop'")
            .EndFor();

        var result = builder.Build();

        Assert.That(result, Is.EqualTo("for i in 1 2 3; do\necho $i && echo 'Loop';\ndone\n"));
    }
}