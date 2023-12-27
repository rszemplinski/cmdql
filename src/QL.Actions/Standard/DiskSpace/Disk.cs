namespace QL.Actions.Standard.DiskSpace;

public class Disk
{
    public string BlockSize { get; set; }
    public string MountPoint { get; set; }
    public string FileSystem { get; set; }
    public uint Total => Used + Available;
    public uint Used { get; set; }
    public uint Available { get; set; }
    public float UsedPercentage => (float)Used / Total * 100f;
    public float AvailablePercentage => (float)Available / Total * 100f;
}