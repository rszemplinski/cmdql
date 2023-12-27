using System.Text.Json.Serialization;

namespace QL.Actions.Standard.FileSystem;

/**
 * Represents a file or directory in the file system
 */
public class FileSystemItem
{
    /**
     * Permissions (e.g. drwxr-xr-x)
     */
    public string Permissions { get; set; }

    /**
     * Number of links (e.g. 1)
     */
    public uint LinkCount { get; set; }

    /**
     * Owner (e.g. root)
     */
    public string Owner { get; set; }
    
    /**
     * Group (e.g. root)
     */
    public string Group { get; set; }

    /**
     * Size in bytes (e.g. 4096)
     */
    public ulong Size { get; set; }

    /**
     * Date (e.g. Jul 10 14:26)
     */
    public DateTime Date { get; set; }
    
    /**
     * Name of file or directory
     */
    public string Name { get; set; }

    /**
     * Is this a directory?
     */
    public bool IsDirectory => Permissions.StartsWith("d");
    
    /**
     * Is this a link?
     */
    public bool IsLink => Permissions.StartsWith("l");
    
    /**
     * Is this a file?
     */
    public bool IsFile => !IsDirectory && !IsLink;
    
    /**
     * Is this a hidden file or directory?
     */
    public bool IsHidden => Name.StartsWith(".");

    /**
     * Is this file executable?
     */
    public bool IsExecutable => Permissions[3] == 'x';
    
    /**
     * Is this file readable?
     */
    public bool IsReadable => Permissions[4] == 'r';
    
    /**
     * Is this file writable?
     */
    public bool IsWritable => Permissions[5] == 'w';
    
    /**
     * The file extension (e.g. .txt)
     */
    public string Extension => !IsDirectory ? Path.GetExtension(Name) : string.Empty;
}