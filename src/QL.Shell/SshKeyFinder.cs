using System.Text.RegularExpressions;

namespace QLShell;

public static class SshKeyFinder
{
    public static string? FindDefaultSshPrivateKey()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Check SSH config file
        var configPath = Path.Combine(homeDir, ".ssh", "config");
        if (File.Exists(configPath))
        {
            var configContent = File.ReadAllText(configPath);
            var regex = new Regex(@"IdentityFile\s+(.*)", RegexOptions.Multiline);
            foreach (Match match in regex.Matches(configContent))
            {
                var keyPath = match.Groups[1].Value;
                if (File.Exists(keyPath))
                {
                    return keyPath;
                }
            }
        }

        var defaultKeyPaths = new[]
        {
            Path.Combine(homeDir, ".ssh", "id_rsa"),
            Path.Combine(homeDir, ".ssh", "id_dsa"),
            Path.Combine(homeDir, ".ssh", "id_ecdsa"),
            Path.Combine(homeDir, ".ssh", "id_ed25519"),
        };

        return defaultKeyPaths.FirstOrDefault(File.Exists);
    }
}