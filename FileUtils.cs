namespace RepoRanger;

public static class FileUtils
{
    public static void DeleteDirectory(string directory)
    {
        foreach (string subdirectory in Directory.EnumerateDirectories(directory))
        {
            DeleteDirectory(subdirectory);
        }

        foreach (string fileName in Directory.EnumerateFiles(directory))
        {
            FileInfo fileInfo = new FileInfo(fileName)
            {
                Attributes = FileAttributes.Normal
            };
            fileInfo.Delete();
        }

        Directory.Delete(directory);
    }
}