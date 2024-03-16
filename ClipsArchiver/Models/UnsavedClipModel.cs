using System.Timers;
using ClipsArchiver.Entities;
using ClipsArchiver.Exceptions;
using ClipsArchiver.Services;
using ClipsArchiver.ViewModels;
using Path = System.IO.Path;
using Timer = System.Timers.Timer;

namespace ClipsArchiver.Models;

public class UnsavedClipModel() : ViewModelBase
{
    private Timer? _timer;
    private int _clipId;
    private readonly string _localFilePath = string.Empty;
    
    private QueueEntry? _queueEntry;
    public QueueEntry? QueueEntry
    {
        get => _queueEntry;
        set => SetField(ref _queueEntry, value);
    }

    private string _localFilename = string.Empty;
    public string LocalFilename
    {
        get => _localFilename;
        set => SetField(ref _localFilename, value);
    }

    private string _status = String.Empty;
    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    private bool _isFinishedUploading;
    public bool IsFinishedUploading
    {
        get => _isFinishedUploading;
        set => SetField(ref _isFinishedUploading, value);
    }

    private bool _hasStartedUploading;
    public bool HasStartedUploading
    {
        get => _hasStartedUploading;
        set => SetField(ref _hasStartedUploading, value);
    }

    private bool _failedUpload;

    public bool FailedUpload
    {
        get => _failedUpload;
        set => SetField(ref _failedUpload, value);
    }

    public UnsavedClipModel(string localFilePath) : this()
    {
        _localFilePath = localFilePath;
        LocalFilename = Path.GetFileName(localFilePath);
        HasStartedUploading = false;
        Status = "ready to upload";
    }

    public async Task UploadClipAsync(bool throttleUpload = false)
    {
        IsFinishedUploading = false;
        HasStartedUploading = true;
        Status = "uploading";
        Settings settings = SettingsService.GetSettings();
        try
        {
            Clip clip = await ClipsRestService.UploadClipAsync(_localFilePath, settings.UserId);
            _clipId = clip.Id;
            LocalClipInfo clipInfo = LocalDbService.GetInfoForFileName(_localFilename);
            clipInfo.ClipId = clip.Id;
            LocalDbService.SetInfoForFileName(clipInfo);
            StartPollQueueStatus();
        }
        catch (ClipExistsUploadException)
        {
            Status = "Clip already uploaded";
            FailedUpload = true;
        }
        catch (ErrorUploadException)
        {
            Status = "Error occured on upload";
            FailedUpload = true;
        }
    }

    private void StartPollQueueStatus()
    {
        _timer = new Timer();
        _timer.Interval = 1000 * 3;
        _timer.AutoReset = true;
        _timer.Elapsed += PollQueueStatus;
        _timer.Start();
    }

    private async void PollQueueStatus(object? sender, ElapsedEventArgs e)
    {
        QueueEntry? queueEntry = await ClipsRestService.GetQueueEntryByClipIdAsync(_clipId);
        if (queueEntry is null)
        {
            return;
        }
        QueueEntry = queueEntry;
        Status = queueEntry?.Status ?? "";
        if (queueEntry?.Status == "finished")
        {
            IsFinishedUploading = true;
            _timer?.Stop();
        }
    }
}