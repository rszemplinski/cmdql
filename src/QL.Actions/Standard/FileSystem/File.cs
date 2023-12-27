using System.Text.Json.Serialization;

namespace QL.Actions.Standard.FileSystem;

public class File
{
    public string Permissions { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public uint LinkCount { get; set; }
    
    public string Owner { get; set; }
    public string Group { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ulong Size { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime Date { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return
            $"Permissions: {Permissions}, LinkCount: {LinkCount}, Owner: {Owner}, Group: {Group}, Size: {Size}, Date: {Date}, Name: {Name}";
    }
}