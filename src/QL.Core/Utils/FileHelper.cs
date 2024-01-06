namespace QL.Core.Utils;

public static class FileHelper
{
    public static FileStream CreateTempFile(string filename, string content)
    {
        var sanitizedFileName = Path.GetFileName(filename);
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + "_" + sanitizedFileName);

        try
        {
            File.WriteAllText(tempFilePath, content);
            return new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred in CreateTempFile: {ex.Message}");
            throw;
        }
    }
}