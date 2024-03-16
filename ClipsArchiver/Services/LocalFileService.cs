using System.IO;
using ClipsArchiver.Entities;

namespace ClipsArchiver.Services;

public static class LocalFileService
{
    public static List<string> GetNewFilesInClipsDir()
    {
        List<LocalClipInfo> allClipInfo = LocalDbService.GetAllClipInfo();
        Settings settings = SettingsService.GetSettings();
        List<string?> allFiles = Directory.GetFiles(settings.ClipsPath).Select(Path.GetFileName).ToList();
        List<string?> knownFiles = allClipInfo.Select(x => x.FileName).ToList();
        List<string?> newFiles = allFiles.Except(knownFiles).ToList();
        return newFiles.Where(x => x != null).Select(x => x!).ToList();
    }
}