using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClipsArchiver.Entities;
using ClipsArchiver.Models;
using ClipsArchiver.Services;
using ClipsArchiver.Views;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace ClipsArchiver.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ClipModel> Clips { get; set; }

    private ClipModel? _selectedClip;
    public ClipModel? SelectedClip
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
        set => SetField(ref _playbackSpeed, value);
    }

    private string _currentTimestamp;

    public string CurrentTimestamp
    {
        get => _currentTimestamp;
        set => SetField(ref _currentTimestamp, value);
    }
    
    private bool _canGoPrevVideo;
    public bool CanGoPrevVideo
    {
        get => _canGoPrevVideo;
        set => SetField(ref _canGoPrevVideo, value);

    }
    
    private bool _canGoNextVideo;
    public bool CanGoNextVideo
    {
        get => _canGoNextVideo;
        set => SetField(ref _canGoNextVideo, value);

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

    public RelayCommand GoBackDayCommand { get; private set; }
    public RelayCommand GoForwardDayCommand { get; private set; }
    public RelayCommand GoBackCommand { get; private set; }
    public RelayCommand PlayVideoCommand { get; private set; }
    public RelayCommand PauseVideoCommand { get; private set; }
    public RelayCommand GoPrevVideoCommand { get; private set; }
    public RelayCommand GoNextVideoCommand { get; private set; }
    public AsyncRelayCommand OpenFileDialogForClipsCommand { get; private set; }
    public AsyncRelayCommand<ContentPresenter> OpenSettingsCommand { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowViewModel()
    {
        GoBackDayCommand = new RelayCommand(GoBackDay, CanGoBackDay);
        GoForwardDayCommand = new RelayCommand(GoForwardDay, CanGoForwardDay);
        GoBackCommand = new RelayCommand(CloseActiveClip);
        PlayVideoCommand = new RelayCommand(PlayVideo);
        PauseVideoCommand = new RelayCommand(PauseVideo);
        GoPrevVideoCommand = new RelayCommand(GoPrevVideo);
        GoNextVideoCommand = new RelayCommand(GoNextVideo);
        OpenFileDialogForClipsCommand = new AsyncRelayCommand(GetClipsForUpload);
        OpenSettingsCommand = new AsyncRelayCommand<ContentPresenter>(OpenSettings);
        SelectedDateTime = DateTime.Now.Hour < 4 ? DateTime.Now.AddDays(-1) : DateTime.Now;
        Clips = new ObservableCollection<ClipModel>();
        LibVlc = new LibVLC();
        MediaPlayer = new MediaPlayer(LibVlc);
        VideoVolume = 100;
        //Cache users for clips to use
        Task.Run(ClipsRestService.GetAllUsersAsync);
    }
    
    private void MediaPlayerOnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        VideoProgress = ((double)MediaPlayer.Time / (double)MediaPlayer.Length) * 100;
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
    
    public void OpenClipForPlay(ClipModel model)
    {
        SelectedClip = model;
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
        
    }

    public void GoNextVideo()
    {
        
    }

    public async Task GetClipsForUpload()
    {
        Settings settings = SettingsService.GetSettings();
        var fileDialog = new OpenFileDialog();
        fileDialog.Multiselect = true;
        fileDialog.Filter = "mp4 files (*.mp4)|*.mp4";
        fileDialog.DefaultDirectory = settings.ClipsPath;
        if (fileDialog.ShowDialog() ?? false)
        {
            IsUploading = true;
            await ClipsRestService.UploadClipsAsync(new List<string>(fileDialog.FileNames));
            IsUploading = false;
        }
    }

    public async Task OpenSettings(object content)
    {
        ContentDialogService dialogService = new();
        ContentDialog contentDialog = new();
        contentDialog.Title = "Settings";
        contentDialog.ContentPresenter = content as ContentPresenter;
        
        var result = await dialogService.ShowAsync(contentDialog, new CancellationToken());
        
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private async Task UpdateClipsForDateAsync()
    {
        List<Clip> clips = await ClipsRestService.GetClipsForDateAsync(SelectedDateTime);
        List<User> users = await ClipsRestService.GetAllUsersAsync();
        Application.Current.Dispatcher.Invoke(() =>
        {
            Clips.Clear();
            clips.ForEach(async x =>
            {
                BitmapImage image = await ClipsRestService.GetThumbnailForClipAsync(x.Id);
                User user = users.FirstOrDefault(u => u.Id == x.OwnerId) ?? new User();
                Clips.Add(new ClipModel(x, image, OpenClipForPlay) { ClipOwner = user });
            });
            Rows = (int)Math.Ceiling(clips.Count / 4d);
        });
        NoClipsForDate = clips.Count == 0;
        List<Task> cacheTasks = [];
        cacheTasks.AddRange(Clips.Select(clip => clip.CacheForPlay()));

        await Task.WhenAll(cacheTasks);
    }
}