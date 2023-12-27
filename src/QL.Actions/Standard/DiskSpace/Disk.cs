namespace QL.Actions.Standard.DiskSpace;

public class Disk
{
    public string BlockSize { get; set; }
    public string MountPoint { get; set; }
    public string FileSystem { get; set; }
    public ulong Total => Used + Free;
    public ulong Used { get; set; }
    public ulong Free { get; set; }
    public float UsedPercentage => (float)Used / Total * 100f;
    public float AvailablePercentage => (float)Free / Total * 100f;
}