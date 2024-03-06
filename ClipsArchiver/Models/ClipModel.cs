using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using ClipsArchiver.Entities;
using ClipsArchiver.Services;
using CommunityToolkit.Mvvm.Input;

namespace ClipsArchiver.Models;

public class ClipModel : INotifyPropertyChanged
{
    private readonly Action<ClipModel> _openClipAction;
    
    private BitmapImage _thumbnail;
    public BitmapImage Thumbnail
    {
        get => _thumbnail;
        set => SetField(ref _thumbnail, value);
    }

    private Clip _clip;

    public Clip Clip
    {
        get => _clip;
        set => SetField(ref _clip, value);
    }

    public double DurationMinutes => Math.Floor(Clip.Duration / 60d);
    public double DurationSeconds => Clip.Duration % 60d;
    public string Duration => $"{DurationMinutes}:{DurationSeconds}";

    private bool _isCachedForPlay;
    public bool IsCachedForPlay 
    { 
        get => _isCachedForPlay; 
        set => SetField(ref _isCachedForPlay, value); 
    }

    private Uri _videoUri;
    public Uri VideoUri
    {
        get => _videoUri; 
        set => SetField(ref _videoUri, value);
    }

    private User _clipOwner;

    public User ClipOwner
    {
        get => _clipOwner;
        set => SetField(ref _clipOwner, value);
    }
    
    public RelayCommand<ClipModel> ShowVideoCommand { get; private set; }
    
    public ClipModel(Clip clip, BitmapImage thumbnail, Action<ClipModel> openClipAction)
    {
        Clip = clip;
        Thumbnail = thumbnail;
        ShowVideoCommand = new RelayCommand<ClipModel>(ShowVideo, CanShowVideo);
        
        _openClipAction = openClipAction;
    }
    
    private void ShowVideo(ClipModel? model)
    {
        if (model is null)
        {
            return;
        }
        _openClipAction.Invoke(model);
    }

    private bool CanShowVideo(ClipModel? model)
    {
        if (model is null)
        {
            return false;
        }

        return model.IsCachedForPlay;
    }

    public async Task CacheForPlay()
    { 
        VideoUri = await ClipsRestService.DownloadVideoByIdAsync(Clip.Id);
        IsCachedForPlay = true;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
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
}