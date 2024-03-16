using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using ClipsArchiver.Constants;
using ClipsArchiver.Entities;
using ClipsArchiver.Exceptions;
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
        Timeout = TimeSpan.FromMinutes(10)
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

    public static async Task<Clip> UploadClipAsync(string clipPath, int userId, bool throttleUpload = true, int retryCount = 0)
    {
        try
        {
            if (await GetClipExistsByFilenameAsync(Path.GetFileName(clipPath)))
            {
                throw new ClipExistsUploadException();
            }

            Log.Debug($"Uploading video file: {clipPath}");
            if (!File.Exists(clipPath))
            {
                throw new ErrorUploadException();
            }

            Stream stream = File.OpenRead(clipPath);
            if (throttleUpload)
            {
                stream = new ThrottledStream(stream, 50000);
            }

            if (FileTypeDetector.DetectFileType(stream) != FileType.Mp4)
            {
                stream.Close();
                throw new ErrorUploadException();
            }

            DateTime dateTime = File.GetCreationTimeUtc(clipPath);

            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(
                new StringContent($"{dateTime.Year}-{dateTime.Month}-{dateTime.Day}-{dateTime.Hour}-{dateTime.Minute}"),
                "creationDateTime");
            content.Add(new StreamContent(stream), "file", Path.GetFileName(clipPath));
            HttpResponseMessage response = await _httpClient.PostAsync($"/clips/upload/{userId}", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new ErrorUploadException();
            }

            Log.Debug($"Got response: {response.StatusCode}");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Clip>(jsonResponse) ?? throw new ErrorUploadException();
        }
        catch (Exception ex)
        {
            Thread.Sleep(1000 * 10);
            if (retryCount > 4)
            {
                throw;
            }
            Log.Error(ex, "Error uploading clip, retrying");
            return await UploadClipAsync(clipPath, userId, throttleUpload, retryCount + 1);
        }
        
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

    public static async Task<List<Tag>> GetAllTagsAsync()
    {
        Log.Debug($"Getting all tags");
        using HttpResponseMessage response = await _httpClient.GetAsync("tags");
        Log.Debug($"Got response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Tag>>(jsonResponse) ?? [];
    }

    public static async Task<QueueEntry?> GetQueueEntryByClipIdAsync(int clipId)
    {
        Log.Debug($"Getting queue entry for clip with id: {clipId}");
        HttpResponseMessage response = await _httpClient.GetAsync($"clips/queue/{clipId}");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<QueueEntry>(jsonResponse);
    }

    public static async Task<bool> GetClipExistsByFilenameAsync(string filename)
    {
        Log.Debug($"Getting clip with filename: {filename}");
        using HttpResponseMessage response = await _httpClient.GetAsync($"clips/filename/{filename}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public static async Task UpdateClipAsync(Clip clip)
    {
        Log.Debug($"Updating clip with id: {clip.Id}");
        using HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"clips/{clip.Id}", clip);
        response.EnsureSuccessStatusCode();
    }
    
    public static async Task DeleteClipAsync(int clipId)
    {
        Log.Debug($"Deleting clip with id: {clipId}");
        using HttpResponseMessage response = await _httpClient.DeleteAsync($"clips/{clipId}");
        response.EnsureSuccessStatusCode();
    }
    
    public static async Task DownloadClipAsync(int clipId, string savePath)
    {
        Log.Debug($"Downloading clip with id: {clipId}");
        using HttpResponseMessage response = await _httpClient.GetAsync($"clips/download/{clipId}");
        response.EnsureSuccessStatusCode();
        await using Stream contentStream = await response.Content.ReadAsStreamAsync();
        await using FileStream fileStream = new(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await contentStream.CopyToAsync(fileStream);
    }
}