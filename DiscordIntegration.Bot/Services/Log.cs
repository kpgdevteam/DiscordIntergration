namespace DiscordIntegration.Bot.Services;

using DSharpPlus;
using DSharpPlus.Entities;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

public class Log
{
    public static string DirectoryPath => Path.Combine(Environment.CurrentDirectory, "logs");

    public static Task Send(ushort port, DiscordMessage msg, bool skipLog = false)
    {
        Console.WriteLine($"{DateTime.Now.Date.ToString("MM/dd/yyyy")} {msg}");
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        string filePath = Path.Combine(DirectoryPath, port == 0 ? "Program.log" : $"{port}.log");
        File.AppendAllText(filePath, $"{DateTime.Now.Date.ToString("MM/dd/yyyy")} {msg}\n");

        if (!skipLog)
        {
            try
            {
                CheckFileSize(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(nameof(Log), $"Error handling log file archival:\n{e}", true);
            }
        }

        return Task.CompletedTask;
    }

    private static void CheckFileSize(string path)
    {
        FileInfo file = new(path);
        // 10485760 = 10 MB
        if (file.Length > 10485760)
        {
            string archivePath = Path.Combine(DirectoryPath, $"{file.Name}.tar.gz");
            if (File.Exists(archivePath))
                File.Delete(archivePath);
            
            using FileStream outStream = File.Create(archivePath);
            using GZipOutputStream gzoStream = new(outStream);
            
            TarArchive? archive = TarArchive.CreateOutputTarArchive(gzoStream);
            archive.RootPath = DirectoryPath.Replace('\\', '/').TrimEnd('/');

            TarEntry entry = TarEntry.CreateEntryFromFile(path);
            entry.Name = path.Replace(DirectoryPath, string.Empty);
            entry.Name = entry.Name.TrimStart('\\');
            
            archive.WriteEntry(entry, true);

            File.Delete(path);
        }
    }
}