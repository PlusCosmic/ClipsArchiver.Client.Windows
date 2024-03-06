using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using ClipsArchiver.Constants;
using ClipsArchiver.Entities;
using LazyCache;
using MetadataExtractor.Util;
using Newtonsoft.Json;

namespace ClipsArchiver.Services;

public class ClipsRestService
{
    
    private static HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://10.0.0.10:8080"),
    };

    private static IAppCache _cache = new CachingService();
    
    public static async Task<List<Clip>> GetClipsForDateAsync(DateTimeOffset date)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync($"clips/date/{date.Year}-{date.Month}-{date.Day}");
    
        response.EnsureSuccessStatusCode();
    
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Clip>>(jsonResponse) ?? [];
    }

    public static async Task<BitmapImage> GetThumbnailForClipAsync(int clipId)
    {
        if (File.Exists(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.png"))
        {
            return new BitmapImage(new Uri(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.png"));
        }
        
        var httpResult = await _httpClient.GetAsync($"clips/download/thumbnail/{clipId}");
        
        httpResult.EnsureSuccessStatusCode();
        
        using var resultStream = await httpResult.Content.ReadAsStreamAsync();
        
        if (!Directory.Exists(Path.GetTempPath() + $@"\ClipsArchiver"))
        {
            Directory.CreateDirectory(Path.GetTempPath() + $@"\ClipsArchiver");
        }
        
        using var fileStream = File.Create(Path.GetTempPath() + $@"\ClipsArchiver\{clipId}.png");
        resultStream.CopyTo(fileStream);
        
        resultStream.Close();
        fileStream.Close();
        
        return new BitmapImage(new Uri(Path.GetTempPath() + $@"\ClipsArchiver\{clipId}.png"));
    }

    public static async Task<Uri> DownloadVideoByIdAsync(int clipId)
    {
        if (File.Exists(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.mp4"))
        {
            return new Uri(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.mp4");
        }
        
        var httpResult = await _httpClient.GetAsync($"clips/download/{clipId}");
        
        httpResult.EnsureSuccessStatusCode();
        
        using var resultStream = await httpResult.Content.ReadAsStreamAsync();
        
        if (!Directory.Exists(Path.GetTempPath() + $@"\ClipsArchiver"))
        {
            Directory.CreateDirectory(Path.GetTempPath() + $@"\ClipsArchiver");
        }
        
        using var fileStream = File.Create(Path.GetTempPath() + $@"\ClipsArchiver\{clipId}.mp4");
        resultStream.CopyTo(fileStream);
        
        resultStream.Close();
        fileStream.Close();
        
        return new Uri(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.mp4");
    }

    public static async Task UploadClipsAsync(List<string> clipPaths)
    {
        Settings settings = SettingsService.GetSettings();
        foreach (var clipPath in clipPaths)
        {
            if (!File.Exists(clipPath))
            {
                continue;
            }

            FileStream stream = File.OpenRead(clipPath);

            if (FileTypeDetector.DetectFileType(stream) != FileType.Mp4)
            {
                stream.Close();
                continue;
            }

            DateTime dateTime = File.GetCreationTimeUtc(clipPath);

            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent($"{dateTime.Year}-{dateTime.Month}-{dateTime.Day}-{dateTime.Hour}-{dateTime.Minute}"), "creationDateTime");
            content.Add(new StreamContent(stream), "file", Path.GetFileName(clipPath));
            await _httpClient.PostAsync($"/clips/upload/{settings.UserId}", content);
        }
    }

    public static async Task<List<User>> GetAllUsersAsync()
    {
        return await _cache.GetOrAddAsync(CacheKeys.AllUsersKey, async() =>
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("users");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<User>>(jsonResponse) ?? [];
        });
    }
}