using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.ListFiles;

public class ListFileArguments
{
    /**
     * The path to list files from
     */
    public string? Path { get; set; }
    
    /**
     * Whether to list files recursively
     */
    public bool Recursive { get; set; }

    /**
     * Whether to include hidden files
     */
    public bool ShowHidden { get; set; }
}

[Action]
[Cmd("ls -l ?[recursive]-R ?[showHidden]-A {path}")]
[Regex(
    @"^(?:^\.\/(.+):)?\s*(?:^total\s+(\d+))?\s*(?<Permissions>[drwx-]+@?)\s+(?<LinkCount>\d+)\s+(?<Owner>[.\w]+)\s+(?<Group>[.\w]+)\s+(?<Size>\d+)\s+(?<Date>\w+\s+\d{1,2}\s+(?:\d+:\d+|\d{4}))\s+(?<Name>.+)$")]
public class ListFiles : ActionBase<ListFileArguments, List<FileSystemItem>>;