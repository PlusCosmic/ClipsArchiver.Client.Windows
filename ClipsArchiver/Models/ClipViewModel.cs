using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media.Imaging;
using ClipsArchiver.Entities;
using ClipsArchiver.Services;
using ClipsArchiver.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace ClipsArchiver.Models;

public class ClipViewModel : ViewModelBase
{
    private readonly Action<ClipViewModel> _openClipAction;
    public LocalClipInfo ClipInfo { get; set; }
    
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
    
    private double DurationMinutes => Math.Floor(Clip.Duration / 60d);
    private double DurationSeconds => Clip.Duration % 60d;
    public string Duration
    {
        get
        {
            string durationMinutes = DurationMinutes >= 10 ? DurationMinutes.ToString(CultureInfo.InvariantCulture) : $"0{DurationMinutes}";
            string durationSeconds = DurationSeconds >= 10
                ? DurationSeconds.ToString(CultureInfo.InvariantCulture)
                : $"0{DurationSeconds}";
            return $"{durationMinutes}:{durationSeconds}";
        }
    }

    private Uri _videoUri;
    public Uri VideoUri
    {
        get => _videoUri; 
        set => SetField(ref _videoUri, value);
    }

    private User? _clipOwner;
    public User? ClipOwner
    {
        get => _clipOwner;
        set => SetField(ref _clipOwner, value);
    }

    private bool _isWatched;
    public bool IsWatched
    {
        get => _isWatched;
        set => SetField(ref _isWatched, value);
    }

    private ObservableCollection<string> _tags;
    public ObservableCollection<string> Tags
    {
        get => _tags;
        set => SetField(ref _tags, value);
    }

    public string ClipName => $"Clip #{Clip.Id}";
    public string UploadedBy => $"Uploaded by: {ClipOwner?.Name}";
    
    public RelayCommand<ClipViewModel> ShowVideoCommand { get; private set; }
    
    public ClipViewModel(Action<ClipViewModel> openClipAction, Clip clip)
    {
        _clip = clip;
        _videoUri = new Uri(clip.VideoUri);
        _thumbnail = new BitmapImage(new Uri(clip.ThumbnailUri));
        ShowVideoCommand = new RelayCommand<ClipViewModel>(ShowVideo);
        _openClipAction = openClipAction;
        ClipInfo = LocalDbService.GetInfoForClipId(clip.Id);
        IsWatched = ClipInfo.Watched;
        _tags = new ObservableCollection<string>();
        clip.Tags?.ForEach(t => Tags.Add(t));
    }
    
    private void ShowVideo(ClipViewModel? model)
    {
        if (model is null)
        {
            return;
        }

        if (!ClipInfo.Watched)
        {
            ClipInfo.Watched = true;
            IsWatched = true;
            LocalDbService.SetInfoForClipId(ClipInfo);
        }
        
        _openClipAction.Invoke(model);
    }

    public async Task SaveClipAsync()
    {
        _clip.Tags = Tags.ToList();
        await ClipsRestService.UpdateClipAsync(_clip);
    }
}