using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
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
    private readonly List<Map> _allMaps;
    private readonly List<Legend> _allLegends;

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
    
    public ObservableCollection<Map> AllMaps { get; set; }
    public ObservableCollection<Legend> AllLegends { get; set; }
    
    private Map _selectedMap;
    public Map SelectedMap
    {
        get => _selectedMap;
        set
        {
            if (SetField(ref _selectedMap, value))
            {
                if (!_clip.Map.Valid || SelectedMap.Id != _clip.Map?.Int32)
                {
                    _clip.Map = new NullInt();
                    _clip.Map.Int32 = SelectedMap.Id;
                    _clip.Map.Valid = true;
                    UpdateClipAsync().RunSynchronously();
                }
            }
        } 
    }

    private Legend _selectedLegend;
    public Legend SelectedLegend
    {
        get => _selectedLegend;
        set
        {
            if (SetField(ref _selectedLegend, value))
            {
                if (!_clip.Legend.Valid || SelectedLegend.Id != _clip.Legend?.Int32)
                {
                    _clip.Legend = new NullInt();
                    _clip.Legend.Int32 = SelectedLegend.Id;
                    _clip.Legend.Valid = true;
                    Task.Run(async () => await UpdateClipAsync());
                }
            }
        } 
    }
    
    public string ClipName => $"Clip #{Clip.Id}";
    public string UploadedBy => $"Uploaded by: {ClipOwner?.Name}";

    private string _mapUri;

    public string MapUri
    {
        get => _mapUri;
        set => SetField(ref _mapUri, value);
    }

    private string _legendUri;
    public string LegendUri 
    {
        get => _legendUri;
        set => SetField(ref _legendUri, value);
    }
    
    public RelayCommand<ClipViewModel> ShowVideoCommand { get; private set; }
    
    public ClipViewModel(Action<ClipViewModel> openClipAction, Clip clip, List<Map> allMaps, List<Legend> allLegends)
    {
        _clip = clip;
        _allMaps = allMaps;
        _allLegends = allLegends;
        AllMaps = new ObservableCollection<Map>(_allMaps);
        AllLegends = new ObservableCollection<Legend>(_allLegends);
        _videoUri = new Uri(clip.VideoUri);
        _thumbnail = new BitmapImage(new Uri(clip.ThumbnailUri));
        ShowVideoCommand = new RelayCommand<ClipViewModel>(ShowVideo);
        _openClipAction = openClipAction;
        ClipInfo = LocalDbService.GetInfoForClipId(clip.Id);
        IsWatched = ClipInfo.Watched;
        _tags = new ObservableCollection<string>();
        clip.Tags?.ForEach(t => Tags.Add(t));
        if (_clip.Map?.Valid ?? false)
        {
            SelectedMap = _allMaps.First(m => m.Id == _clip.Map.Int32);
            MapUri = $"http://10.0.0.10:8080/resources/{SelectedMap.CardImage}";
        }
        if (_clip.Legend?.Valid ?? false)
        {
            SelectedLegend = _allLegends.First(l => l.Id == _clip.Legend.Int32);
            LegendUri = $"http://10.0.0.10:8080/resources/{SelectedLegend.CardImage}";
        }
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

    public async Task UpdateClipAsync()
    {
        if (_clip.Map.Valid || _clip.Legend.Valid)
        {
            _clip.MatchHistoryFound = true;
        }
        await ClipsRestService.UpdateClipAsync(_clip);
        Clip newClip = await ClipsRestService.GetClipByIdAsync(_clip.Id);
        Application.Current.Dispatcher.Invoke(() =>
        {
            Clip = newClip;
            Tags.Clear();
            newClip.Tags?.ForEach(t => Tags.Add(t));
            SelectedMap = _allMaps.First(m => m.Id == _clip.Map.Int32);
            MapUri = $"http://10.0.0.10:8080/resources/{SelectedMap.CardImage}";
            SelectedLegend = _allLegends.First(l => l.Id == _clip.Legend.Int32);
            LegendUri = $"http://10.0.0.10:8080/resources/{SelectedLegend.CardImage}";
        });
    }
}