using QL.Actions.Core.Actions;
using QL.Actions.Core.Attributes;

namespace QL.Actions.Standard.FileSystem;

public class ListFileArguments
{
    public string Path { get; set; }
    public bool Recursive { get; set; }
    
    public override string ToString()
    {
        return $"Path: {Path}, Recursive: {Recursive}";
    }
}

[Action]
[Cmd("ls -la ?[recursive]-R {path}")]
[Regex(
    @"^(?:^\.\/(.+):)?\s*(?:^total\s+(\d+))?\s*(?<permissions>[drwx-]+@?)\s+(?<linkCount>\d+)\s+(?<owner>[.\w]+)\s+(?<group>[.\w]+)\s+(?<size>\d+)\s+(?<date>\w+\s+\d{1,2}\s+(?:\d+:\d+|\d{4}))\s+(?<name>.+)$")]
public class ListFiles : ActionBase<ListFileArguments, List<File>>;
