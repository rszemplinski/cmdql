using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.DiskSpace;

[Action]
[Cmd("df")]
public class DiskSpace : ActionBase<List<Disk>>
{
    protected override List<Disk> ParseCommandResults(ICommandOutput commandResults)
    {
        var disks = new List<Disk>();

        var lines = commandResults.Result.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var blockSize = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];

        foreach (var line in lines.Skip(1))
        {
            var disk = new Disk();
            var values = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            disk.BlockSize = blockSize;
            disk.MountPoint = values.Last();
            disk.FileSystem = values[0];
            // Needs to be in bytes
            disk.Used = ulong.Parse(values[2]) * 1024;
            disk.Free = ulong.Parse(values[3]) * 1024;
            disks.Add(disk);
        }

        return disks;
    }
}