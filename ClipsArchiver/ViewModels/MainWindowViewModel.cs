using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using ClipsArchiver.Entities;
using ClipsArchiver.Models;
using ClipsArchiver.Services;
using ClipsArchiver.Windows;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace ClipsArchiver.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private Timer? _timer;
    private readonly UploadViewModel _uploadViewModel;

    private ObservableCollection<ClipViewModel> _clips;
    public ObservableCollection<ClipViewModel> Clips
    {
        get => _clips; 
        set => SetField(ref _clips, value);
    }

    private ClipViewModel? _selectedClip;
    public ClipViewModel? SelectedClip
    {
        get => _selectedClip;
        set => SetField(ref _selectedClip, value);
    }

    private DateTime _selectedDateTime;
    public DateTime SelectedDateTime
    {
        get => _selectedDateTime;
        set
        {
            if (SetField(ref _selectedDateTime, value))
            {
                Task.Run(UpdateClipsForDateAsync);
            }
        }
    }

    private int _rows;
    public int Rows
    {
        get => _rows;
        set => SetField(ref _rows, value);
    }

    private bool _showingVideo;
    public bool ShowingVideo
    {
        get => _showingVideo; 
        set => SetField(ref _showingVideo, value);
    }
    
    private bool _noClipsForDate;
    public bool NoClipsForDate
    {
        get => _noClipsForDate;
        set => SetField(ref _noClipsForDate, value);
    }
    
    private MediaPlayer _mediaPlayer;
    public MediaPlayer MediaPlayer
    {
        get => _mediaPlayer;
        set => SetField(ref _mediaPlayer, value);
    }

    private LibVLC _libVlc;
    public LibVLC LibVlc
    {
        get => _libVlc;
        set => SetField(ref _libVlc, value);
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetField(ref _isPlaying, value);
    }
    
    private bool _isScrubbing;
    public bool IsScrubbing
    {
        get => _isScrubbing;
        set
        {
            if (SetField(ref _isScrubbing, value))
            {
                if (IsScrubbing)
                {
                    PauseVideo();
                }
                else
                {
                    PlayVideo();
                }
            }
        }
    }

    private double _videoProgress;
    public double VideoProgress
    {
        get => _videoProgress;
        set
        {
            if (SetField(ref _videoProgress, value))
            {
                if (IsScrubbing)
                {
                    MediaPlayer.SeekTo(new TimeSpan(0,0,0,0,(int)((VideoProgress / 100d) * MediaPlayer.Length)));
                }
            }
            
        }
    }

    private double _playbackSpeed;
    public double PlaybackSpeed
    {
        get => _playbackSpeed;
        set
        {
            if(SetField(ref _playbackSpeed, value))
            {
                MediaPlayer.SetRate((float)PlaybackSpeed / 100);
            }
        }
    }

    private string? _currentTimestamp;

    public string? CurrentTimestamp
    {
        get => _currentTimestamp;
        set => SetField(ref _currentTimestamp, value);
    }
    
    private int _videoVolume;
    public int VideoVolume
    {
        get => _videoVolume;
        set
        {
            if (SetField(ref _videoVolume, value))
            {
                MediaPlayer.Volume = value;
            }
        }
    }

    private bool _isUploading;
    public bool IsUploading
    {
        get => _isUploading;
        set => SetField(ref _isUploading, value);
    }
    
    private bool _isLoadingClipsForDate;
    public bool IsLoadingClipsForDate
    {
        get => _isLoadingClipsForDate;
        set => SetField(ref _isLoadingClipsForDate, value);
    }

    private ObservableCollection<string> _allTags;
    public ObservableCollection<string> AllTags
    {
        get => _allTags;
        set => SetField(ref _allTags, value);
    }

    private bool _isControlled;
    public bool IsControlled
    {
        get => _isControlled;
        set => SetField(ref _isControlled, value);
    }
    
    private bool _isFlyoutOpen;
    public bool IsFlyoutOpen
    {
        get => _isFlyoutOpen;
        set => SetField(ref _isFlyoutOpen, value);
    }

    public RelayCommand GoBackDayCommand { get; private set; }
    public RelayCommand GoForwardDayCommand { get; private set; }
    public RelayCommand GoBackCommand { get; private set; }
    public RelayCommand PlayVideoCommand { get; private set; }
    public RelayCommand PauseVideoCommand { get; private set; }
    public RelayCommand GoPrevVideoCommand { get; private set; }
    public RelayCommand GoNextVideoCommand { get; private set; }
    public AsyncRelayCommand DownloadClipCommand { get; private set; }
    public AsyncRelayCommand DeleteClipCommand { get; private set; }
    public AsyncRelayCommand<string> AddTagToSelectedClipCommand { get; private set; }
    public AsyncRelayCommand<string> RemoveTagFromSelectedClipCommand { get; private set; }
    public RelayCommand OpenSettingsWindowCommand { get; private set; }
    public RelayCommand OpenUploadWindowCommand { get; private set; }
    public RelayCommand OpenFlyoutCommand { get; private set; }
    public RelayCommand TogglePlaybackCommand { get; private set; }
    public RelayCommand SkipFiveSecondsBackCommand { get; private set; }
    public RelayCommand SkipFiveSecondsForwardCommand { get; private set; }

    public MainWindowViewModel()
    {
        _clips = new ObservableCollection<ClipViewModel>();
        _allTags = new ObservableCollection<string>();
        _uploadViewModel = new();
        GoBackDayCommand = new RelayCommand(GoBackDay, CanGoBackDay);
        GoForwardDayCommand = new RelayCommand(GoForwardDay, CanGoForwardDay);
        GoBackCommand = new RelayCommand(CloseActiveClip, () => ShowingVideo);
        PlayVideoCommand = new RelayCommand(PlayVideo);
        PauseVideoCommand = new RelayCommand(PauseVideo);
        GoPrevVideoCommand = new RelayCommand(GoPrevVideo);
        GoNextVideoCommand = new RelayCommand(GoNextVideo);
        AddTagToSelectedClipCommand = new AsyncRelayCommand<string>(AddTagToSelectedClip);
        RemoveTagFromSelectedClipCommand = new AsyncRelayCommand<string>(RemoveTagFromSelectedClip);
        OpenSettingsWindowCommand = new RelayCommand(OpenSettingsWindow);
        OpenUploadWindowCommand = new RelayCommand(OpenUploadWindow);
        DownloadClipCommand = new AsyncRelayCommand(DownloadClipAsync);
        DeleteClipCommand = new AsyncRelayCommand(DeleteClipAsync);
        OpenFlyoutCommand = new RelayCommand(() => IsFlyoutOpen = !IsFlyoutOpen);
        TogglePlaybackCommand = new RelayCommand(TogglePlayback, () => ShowingVideo);
        SkipFiveSecondsBackCommand = new RelayCommand(SkipFiveSecondsBack, () => ShowingVideo);
        SkipFiveSecondsForwardCommand = new RelayCommand(SkipFiveSecondsForward, () => ShowingVideo);
        SelectedDateTime = DateTime.Now.Hour < 4 ? DateTime.Now.AddDays(-1) : DateTime.Now;
        Clips = new ObservableCollection<ClipViewModel>();
        AllTags = new ObservableCollection<string>();
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(LibVlc);
        VideoVolume = 100;
        PlaybackSpeed = 100;
        Settings settings = SettingsService.GetSettings();
        if(settings.ShouldWatchClipsPath)
        {
            StartPollWatchFolder();
        }
        else
        {
            StopPollWatchFolder();
        }
        
        //Cache users for clips to use
        Task.Run(ClipsRestService.GetAllUsersAsync);
        Task.Run(async() =>
        {
            List<Tag> allTags = await ClipsRestService.GetAllTagsAsync();
            allTags.ForEach(t => AllTags.Add(t.Name));
        });
    }
    
    private void MediaPlayerOnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        VideoProgress = ((double)MediaPlayer.Time / MediaPlayer.Length) * 100d;
        double minutes = Math.Floor(MediaPlayer.Time / (1000d * 60)); 
        double seconds = Math.Floor((MediaPlayer.Time % (1000d * 60)) / 1000d);
        string minutesString = (minutes < 10 ? "0" : "") + minutes;
        string secondsString = (seconds < 10 ? "0" : "") + seconds;
        CurrentTimestamp = $"{minutesString}:{secondsString}";
    }

    private void GoBackDay()
    {
        SelectedDateTime = SelectedDateTime.AddDays(-1);
    }

    private bool CanGoBackDay()
    {
        return true;
    }

    private void GoForwardDay()
    {
        SelectedDateTime = SelectedDateTime.AddDays(1);
    }

    private bool CanGoForwardDay()
    {
        return true;
    }

    private void OpenClipForPlay(ClipViewModel viewModel)
    {
        SelectedClip = viewModel;
        ShowingVideo = true;
        
        MediaPlayer.Media = new Media(LibVlc, SelectedClip.VideoUri);
        MediaPlayer.TimeChanged += MediaPlayerOnTimeChanged;
        PlayVideo(true);
    }
    
    public void CloseActiveClip()
    {
        MediaPlayer.Stop();
        MediaPlayer.TimeChanged -= MediaPlayerOnTimeChanged;
        SelectedClip = null;
        ShowingVideo = false;
    }
    
    public void PlayVideo(bool restartPlayback)
    {
        if (MediaPlayer.IsPlaying)
        {
            return;
        }
        if (restartPlayback)
        {
            MediaPlayer.Position = 0f;
        }
        MediaPlayer.Play();
        IsPlaying = true;
    }

    public void PlayVideo()
    {
        PlayVideo(false);
    }

    public void PauseVideo()
    {
        if (!MediaPlayer.IsPlaying)
        {
            return;
        }
        MediaPlayer.Pause();
        IsPlaying = false;
    }

    public void GoPrevVideo()
    {
        if (SelectedClip == null)
        {
            return;
        }
        
        int idx = Clips.IndexOf(SelectedClip);
        CloseActiveClip();
        if (idx > 0)
        {
            ClipViewModel next = Clips[idx + 1];
            if (!next.ClipInfo.Watched)
            {
                next.ClipInfo.Watched = true;
                next.IsWatched = true;
                LocalDbService.SetInfoForClipId(next.ClipInfo);
            }
            OpenClipForPlay(Clips[idx - 1]);
        }
    }

    public void GoNextVideo()
    {
        if (SelectedClip == null)
        {
            return;
        }
        
        int idx = Clips.IndexOf(SelectedClip);
        CloseActiveClip();
        if (idx < Clips.Count - 1)
        {
            ClipViewModel next = Clips[idx + 1];
            if (!next.ClipInfo.Watched)
            {
                next.ClipInfo.Watched = true;
                next.IsWatched = true;
                LocalDbService.SetInfoForClipId(next.ClipInfo);
            }
            OpenClipForPlay(next);
        }
    }
    
    private async Task UpdateClipsForDateAsync()
    {
        List<Clip> clips = await ClipsRestService.GetClipsForDateAsync(SelectedDateTime);
        List<User> users = await ClipsRestService.GetAllUsersAsync();
        List<Map> maps = await ClipsRestService.GetAllMapsAsync();
        List<Legend> legends = await ClipsRestService.GetAllLegendsAsync();
        Application.Current.Dispatcher.Invoke(() =>
        {
            Clips.Clear();
            clips.ForEach(x =>
            {
                User user = users.FirstOrDefault(u => u.Id == x.OwnerId) ?? new User();
                Clips.Add(new ClipViewModel(OpenClipForPlay, x, maps, legends) { ClipOwner = user });
            });
            Rows = (int)Math.Ceiling(clips.Count / 4d);
            Clips = new ObservableCollection<ClipViewModel>(Clips.OrderBy(x => x.Clip.CreatedOn?.Time));
        });
        NoClipsForDate = clips.Count == 0;
    }

    private async Task AddTagToSelectedClip(string? tag)
    {
        if(tag is not null && SelectedClip is not null)
        {
            if (!SelectedClip.Tags.Contains(tag))
            {
                SelectedClip.Tags.Add(tag);
                await SelectedClip.SaveClipAsync();
                if (!AllTags.Contains(tag))
                {
                    AllTags.Clear();
                    List<Tag> allTags = await ClipsRestService.GetAllTagsAsync();
                    allTags.ForEach(t => AllTags.Add(t.Name));
                }
            }
        }
    }

    private async Task RemoveTagFromSelectedClip(string? tag)
    {
        if(tag is not null && SelectedClip is not null)
        {
            if (SelectedClip.Tags.Remove(tag))
            {
                await SelectedClip.SaveClipAsync();
            }
        }
    }
    
    private void OpenSettingsWindow()
    {
        SettingsWindow settingsWindow = new();
        settingsWindow.ShowDialog();
        Settings settings = SettingsService.GetSettings();
        if(settings.ShouldWatchClipsPath)
        {
            StartPollWatchFolder();
        }
        else
        {
            StopPollWatchFolder();
        }
    }

    private void OpenUploadWindow()
    {
        UploadWindow uploadWindow = new();
        uploadWindow.DataContext = _uploadViewModel;
        uploadWindow.ShowDialog();
    }
    
    private void StartPollWatchFolder()
    {
        if(_timer is not null && _timer.Enabled)
        {
            return;
        }
        _timer = new Timer();
        _timer.Interval = 1000 * 5;
        _timer.AutoReset = true;
        _timer.Elapsed += PollWatchFolder;
        _timer.Start();
    }
    
    private void StopPollWatchFolder()
    {
        _timer?.Stop();
    }

    private async void PollWatchFolder(object? sender, ElapsedEventArgs e)
    {
        Settings settings = SettingsService.GetSettings();
        var newFiles = LocalFileService.GetNewFilesInClipsDir();
        List<Task> tasks = new();
        foreach (var newFile in newFiles)
        {
            tasks.Add(_uploadViewModel.AddNewClipAndUploadAsync(settings.ClipsPath + "\\" + newFile));
        }

        await Task.WhenAll(tasks);
    }

    private async Task DownloadClipAsync()
    {
        if (SelectedClip is null)
        {
            return;
        }

        OpenFolderDialog dialog = new();
        if (dialog.ShowDialog() == true)
        {
            await ClipsRestService.DownloadClipAsync(SelectedClip.Clip.Id, dialog.FolderName);
        }
    }

    private async Task DeleteClipAsync()
    {
        if(SelectedClip is null)
        {
            return;
        }
        
        await ClipsRestService.DeleteClipAsync(SelectedClip.Clip.Id);
        if (ShowingVideo)
        {
            GoNextVideo();
        }
        Clips.Remove(SelectedClip);
    }
    
    private void TogglePlayback()
    {
        if (IsPlaying)
        {
            PauseVideo();
        }
        else
        {
            PlayVideo();
        }
    }
    
    private void SkipFiveSecondsBack()
    {
        PauseVideo();
        MediaPlayer.Time -= 5000;
        PlayVideo();
    }
    
    private void SkipFiveSecondsForward()
    {
        PauseVideo();
        MediaPlayer.Time += 5000;
        PlayVideo();
    }
}