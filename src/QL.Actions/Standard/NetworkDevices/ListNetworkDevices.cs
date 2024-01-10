using System.Text.RegularExpressions;
using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.NetworkDevices;

public class ReceptionStats
{
    public ulong Packets { get; set; }
    public ulong Bytes { get; set; }
    public ulong Errors { get; set; }
    public ulong Dropped { get; set; }
    public ulong Overruns { get; set; }
    public ulong Frame { get; set; }
}

public class TransmissionStats
{
    public ulong Packets { get; set; }
    public ulong Bytes { get; set; }
    public ulong Errors { get; set; }
    public ulong Dropped { get; set; }
    public ulong Overruns { get; set; }
    public ulong Carrier { get; set; }
    public ulong Collisions { get; set; }
}

public class DeviceResult
{
    public string Name { get; set; }
    public string? IPv4 { get; set; }
    public string? IPv6 { get; set; }
    public string? MAC { get; set; }
    public string? Netmask { get; set; }
    public string? Broadcast { get; set; }
    public ushort PrefixLength { get; set; }
    public List<string> Flags { get; set; }
    public string? ScopeID { get; set; }
    public uint MTU { get; set; }
    public TransmissionStats? Tx { get; set; }
    public ReceptionStats? Rx { get; set; }
}

[Action]
[Cmd("ifconfig")]
public partial class ListNetworkDevices : ActionBase<List<DeviceResult>>
{
    protected override List<DeviceResult> ParseCommandResults(ICommandOutput commandResults)
    {
        return Platform switch
        {
            Platform.Linux => ParseLinux(commandResults),
            Platform.OSX => ParseOSX(commandResults),
            _ => throw new PlatformNotSupportedException()
        };
    }

    private static List<DeviceResult> ParseLinux(ICommandOutput commandResults)
    {
        var regex = LinuxRegex();

        var deviceResults = new List<DeviceResult>();
        var matches = regex.Matches(commandResults.Result);

        foreach (Match match in matches)
        {
            var receptionStats = new ReceptionStats
            {
                Packets = ulong.Parse(match.Groups[12].Value),
                Bytes = ulong.Parse(match.Groups[13].Value),
                Errors = ulong.Parse(match.Groups[14].Value),
                Dropped = ulong.Parse(match.Groups[15].Value),
                Overruns = ulong.Parse(match.Groups[16].Value),
                Frame = ulong.Parse(match.Groups[17].Value),
            };

            var transmissionStats = new TransmissionStats
            {
                Packets = ulong.Parse(match.Groups[18].Value),
                Bytes = ulong.Parse(match.Groups[19].Value),
                Errors = ulong.Parse(match.Groups[20].Value),
                Dropped = ulong.Parse(match.Groups[21].Value),
                Overruns = ulong.Parse(match.Groups[22].Value),
                Carrier = ulong.Parse(match.Groups[23].Value),
                Collisions = ulong.Parse(match.Groups[24].Value),
            };

            var deviceResult = new DeviceResult
            {
                Name = match.Groups[1].Value,
                Flags = [..match.Groups[3].Value.Split(',')],
                MTU = uint.Parse(match.Groups[4].Value),
                IPv4 = match.Groups[5].Success ? match.Groups[5].Value : null,
                Netmask = match.Groups[6].Success ? match.Groups[6].Value : null,
                Broadcast = match.Groups[7].Success ? match.Groups[7].Value : null,
                IPv6 = match.Groups[8].Success ? match.Groups[8].Value : null,
                PrefixLength = match.Groups[9].Success ? ushort.Parse(match.Groups[9].Value) : (ushort)0,
                ScopeID = match.Groups[10].Success ? match.Groups[10].Value : null,
                MAC = match.Groups[11].Success ? match.Groups[11].Value : null,
                Rx = receptionStats,
                Tx = transmissionStats,
            };

            deviceResults.Add(deviceResult);
        }

        return deviceResults;
    }

    // TODO: This is not 100% working. Parses interfaces, flags, and MTU, but not the rest.
    private static List<DeviceResult> ParseOSX(ICommandOutput commandResults)
    {
        var regex = MacRegex();
        var matches = regex.Matches(commandResults.Result);

        var deviceResults = new List<DeviceResult>();

        foreach (Match match in matches)
        {
            var networkInterface = new DeviceResult
            {
                Name = match.Groups[1].Value,
                Flags = [..match.Groups[3].Value.Split(',')],
                MTU = uint.Parse(match.Groups[4].Value),
                IPv4 = match.Groups[5].Success ? match.Groups[5].Value : null,
                Netmask = match.Groups[6].Success ? ConvertToDecimalNetmask(match.Groups[6].Value) : null,
                Broadcast = match.Groups[7].Success ? match.Groups[7].Value : null,
                IPv6 = match.Groups[8].Success ? match.Groups[8].Value : null,
                PrefixLength = match.Groups[9].Success ? ushort.Parse(match.Groups[9].Value) : (ushort)0,
                ScopeID = match.Groups[10].Success ? match.Groups[10].Value : null,
                MAC = match.Groups[11].Success ? match.Groups[11].Value : null
            };

            deviceResults.Add(networkInterface);
        }

        return deviceResults;
    }

    private static string? ConvertToDecimalNetmask(string hexNetmask)
    {
        return uint.TryParse(hexNetmask, System.Globalization.NumberStyles.HexNumber, null, out var netmask)
            ? $"{netmask >> 24}.{(netmask >> 16) & 0xFF}.{(netmask >> 8) & 0xFF}.{netmask & 0xFF}"
            : null;
    }

    [GeneratedRegex(
        @"(\w+): flags=(\d+)<([A-Z,]+)>.*?mtu (\d+).*?(?:inet (\d+\.\d+\.\d+\.\d+).*?netmask (\d+\.\d+\.\d+\.\d+).*?broadcast (\d+\.\d+\.\d+\.\d+).*?)?(?:inet6 ([\da-fA-F:]+).*?prefixlen (\d+).*?scopeid (\w+)<.*?>.*?)*(?:ether ([\da-fA-F:]+) .*?)*RX packets (\d+)  bytes (\d+).*?RX errors (\d+)  dropped (\d+)  overruns (\d+)  frame (\d+).*?TX packets (\d+)  bytes (\d+).*?TX errors (\d+)  dropped (\d+) overruns (\d+)  carrier (\d+)  collisions (\d+)",
        RegexOptions.Singleline)]
    private static partial Regex LinuxRegex();
    [GeneratedRegex("(\\w+): flags=(\\d+)<([A-Z,]+)> mtu (\\d+).*?(?:\\s+inet (\\d+\\.\\d+\\.\\d+\\.\\d+) netmask (0x[a-fA-F0-9]+) broadcast (\\d+\\.\\d+\\.\\d+\\.\\d+).*?)?(?:\\s+inet6 ([\\da-fA-F:]+)%?\\w* prefixlen (\\d+).*?(?:scopeid (0x\\w+))?)?(?:\\s+ether ([\\da-fA-F:]+).*?)?(?:\\s+media: .*\\n\\s+status: .*)*", RegexOptions.Singleline)]
    private static partial Regex MacRegex();
}