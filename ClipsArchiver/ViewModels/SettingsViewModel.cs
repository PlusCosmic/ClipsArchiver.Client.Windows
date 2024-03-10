using System.Collections.ObjectModel;
using System.Reflection;
using ClipsArchiver.Entities;
using ClipsArchiver.Services;
using CommunityToolkit.Mvvm.Input;
using Velopack;
using Velopack.Sources;
using Wpf.Ui.Controls;

namespace ClipsArchiver.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private User? _selectedUser;
    public User? SelectedUser
    {
        get => _selectedUser;
        set => SetField(ref _selectedUser, value);
    }

    private ObservableCollection<User>? _availableUsers;

    public ObservableCollection<User>? AvailableUsers
    {
        get => _availableUsers;
        set => SetField(ref _availableUsers, value);
    }

    private string? _clipsPath;
    public string? ClipsPath
    {
        get => _clipsPath;
        set => SetField(ref _clipsPath, value);
    }

    private string _updateStatus;
    public string UpdateStatus
    {
        get => _updateStatus;
        set => SetField(ref _updateStatus, value);
    }

    private string _currentVersion;
    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetField(ref _currentVersion, value);
    }
    
    private bool _shouldWatchFolder;
    public bool ShouldWatchFolder
    {
        get => _shouldWatchFolder;
        set => SetField(ref _shouldWatchFolder, value);
    }

    public AsyncRelayCommand CheckForUpdatesCommand { get; private set; }
    public RelayCommand<FluentWindow> SaveSettingsCommand { get; private set; }
    
    public SettingsViewModel()
    {
        CheckForUpdatesCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
        SaveSettingsCommand = new RelayCommand<FluentWindow>(SaveSettings);
        CurrentVersion = "v" + Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0";
        Task.Run(InitAsync);
    }

    private async Task InitAsync()
    {
        Settings settings = SettingsService.GetSettings();
        List<User> users = await ClipsRestService.GetAllUsersAsync();
        AvailableUsers = new ObservableCollection<User>(users);
        ClipsPath = settings.ClipsPath;

        SelectedUser = settings.UserId == 0 ? null : AvailableUsers.First(x => x.Id == settings.UserId);
        ShouldWatchFolder = settings.ShouldWatchClipsPath;
    }
    
    private async Task CheckForUpdatesAsync()
    {
        var mgr = new UpdateManager(new GithubSource("https://github.com/PlusCosmic/ClipsArchiver.Client.Windows", null, false));

        // check for new version
        UpdateStatus = "Checking for updates...";
        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null){
            UpdateStatus = "No updates available";
            return; // no update available
        }

        UpdateStatus = "Downloading new version...";
        // download new version
        await mgr.DownloadUpdatesAsync(newVersion);

        UpdateStatus = "Installing new version...";
        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion);
    }

    private void SaveSettings(FluentWindow? window)
    {
        Settings settings = new()
        {
            UserId = SelectedUser?.Id ?? 0,
            ClipsPath = ClipsPath ?? string.Empty,
            ShouldWatchClipsPath = ShouldWatchFolder
        };
        SettingsService.SaveSettings(settings);
        window?.Close();
    }
}