using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.FileSpace;

public class FileSpaceArguments
{
    public string Path { get; set; } = "~";
    public int Depth { get; set; } = -1;
}

public class FileSpaceResult
{
    public ulong Size { get; set; }
    public string Path { get; set; }
}

[Action]
public class FileSpace : ActionBase<FileSpaceArguments, List<FileSpaceResult>>
{
    protected override string BuildCommand(FileSpaceArguments arguments)
    {
        var depth = arguments.Depth >= 0 ? $"-d {arguments.Depth}" : "";
        var command = $"du {depth} {arguments.Path}";
        return command;
    }

    protected override List<FileSpaceResult> ParseCommandResults(ICommandOutput commandResults)
    {
        var results = new List<FileSpaceResult>();
        var lines = commandResults.Result.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var values = line.Split("\t", StringSplitOptions.RemoveEmptyEntries);
            var size = ulong.Parse(values[0]);
            var path = values[1];
            results.Add(new FileSpaceResult
            {
                Size = size, Path = path
            });
        }

        return results;
    }
}