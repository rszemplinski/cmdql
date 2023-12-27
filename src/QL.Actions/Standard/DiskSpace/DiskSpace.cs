using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.DiskSpace;

public class DiskSpaceArguments;

[Action]
[Cmd("df")]
public class DiskSpace : ActionBase<DiskSpaceArguments, List<Disk>>
{
    /**
     * Example output:
     * Filesystem     1K-blocks     Used Available Use% Mounted on
     * udev             16353532        0  16353532   0% /dev
     * tmpfs             3273896     1576   3272320   1% /run
     */
    protected override Task<List<Disk>> _ParseCommandResultsAsync(string commandResults)
    {
        var disks = new List<Disk>();
        var lines = commandResults.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var blockSize = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
        
        foreach (var line in lines.Skip(1))
        {
            var disk = new Disk();
            var values = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            disk.BlockSize = blockSize;
            disk.MountPoint = values.Last();
            disk.FileSystem = values[0];
            disk.Used = ulong.Parse(values[2]);
            disk.Free = ulong.Parse(values[3]);
            disks.Add(disk);
        }

        return Task.FromResult(disks);
    }
}