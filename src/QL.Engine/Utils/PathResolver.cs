namespace QL.Engine.Utils;

public static class PathResolver
{
    public static string GetFullPath(string path)
    {
        if (!path.StartsWith("~")) return Path.GetFullPath(path);
        // Get the home directory
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // Remove the tilde and combine with home directory
        return Path.Combine(home, path.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));

    }
}