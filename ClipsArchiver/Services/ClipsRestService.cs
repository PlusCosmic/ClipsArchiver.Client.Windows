using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using ClipsArchiver.Constants;
using ClipsArchiver.Entities;
using LazyCache;
using MetadataExtractor.Util;
using Newtonsoft.Json;
using Serilog;

namespace ClipsArchiver.Services;

public class ClipsRestService
{
    
    private static HttpClient _httpClient = new(new HttpClientHandler
    {
        MaxConnectionsPerServer = 100
    })
    {
        BaseAddress = new Uri("http://10.0.0.10:8080"),
    };

    private static IAppCache _cache = new CachingService();
    
    public static async Task<List<Clip>> GetClipsForDateAsync(DateTimeOffset date)
    {
        Log.Debug($"Getting clips for date: {date.Date}");
        using HttpResponseMessage response = await _httpClient.GetAsync($"clips/date/{date.Year}-{date.Month}-{date.Day}");
        Log.Debug($"Got response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();
    
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Clip>>(jsonResponse) ?? [];
    }

    public static async Task<BitmapImage> GetThumbnailForClipAsync(int clipId)
    {
        Log.Debug($"Getting thumbnail for clip: {clipId}");
        if (File.Exists(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.png"))
        {
            Log.Debug($"Found thumbnail in cache: {Path.GetTempPath() + $@"ClipsArchiver\{clipId}.png"}");
            return new BitmapImage(new Uri(Path.GetTempPath() + $@"ClipsArchiver\{clipId}.png"));
        }
        var response = await _httpClient.GetAsync($"clips/download/thumbnail/{clipId}");
        Log.Debug($"Got response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();
        
        using var resultStream = await response.Content.ReadAsStreamAsync();
        
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

    public static async Task UploadClipsAsync(List<string> clipPaths)
    {
        Settings settings = SettingsService.GetSettings();
        List<Task> tasks = new();
        foreach (var clipPath in clipPaths)
        {
            tasks.Add(UploadClipAsync(clipPath, settings.UserId));
        }

        await Task.WhenAll(tasks);
    }

    private static async Task UploadClipAsync(string clipPath, int userId)
    {
        Log.Debug($"Uploading video file: {clipPath}");
        if (!File.Exists(clipPath))
        {
            return;
        }

        FileStream stream = File.OpenRead(clipPath);

        if (FileTypeDetector.DetectFileType(stream) != FileType.Mp4)
        {
            stream.Close();
            return;
        }

        DateTime dateTime = File.GetCreationTimeUtc(clipPath);

        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent($"{dateTime.Year}-{dateTime.Month}-{dateTime.Day}-{dateTime.Hour}-{dateTime.Minute}"), "creationDateTime");
        content.Add(new StreamContent(stream), "file", Path.GetFileName(clipPath));
        HttpResponseMessage response = await _httpClient.PostAsync($"/clips/upload/{userId}", content);
        Log.Debug($"Got response: {response.StatusCode}");
    }

    public static async Task<List<User>> GetAllUsersAsync()
    {
        return await _cache.GetOrAddAsync(CacheKeys.AllUsersKey, async() =>
        {
            Log.Debug($"Getting all users");
            using HttpResponseMessage response = await _httpClient.GetAsync("users");
            Log.Debug($"Got response: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<User>>(jsonResponse) ?? [];
        });
    }
}