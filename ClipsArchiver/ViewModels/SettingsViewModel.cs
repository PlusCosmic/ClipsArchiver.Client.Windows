using System.Collections.ObjectModel;
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

    public AsyncRelayCommand CheckForUpdatesCommand { get; private set; }
    public RelayCommand<FluentWindow> SaveSettingsCommand { get; private set; }
    
    public SettingsViewModel()
    {
        CheckForUpdatesCommand = new AsyncRelayCommand(CheckForUpdatesAsync);
        SaveSettingsCommand = new RelayCommand<FluentWindow>(SaveSettings);
        Task.Run(InitAsync);
    }

    private async Task InitAsync()
    {
        Settings settings = SettingsService.GetSettings();
        List<User> users = await ClipsRestService.GetAllUsersAsync();
        AvailableUsers = new ObservableCollection<User>(users);
        ClipsPath = settings.ClipsPath;

        SelectedUser = settings.UserId == 0 ? null : AvailableUsers.First(x => x.Id == settings.UserId);
    }
    
    private async Task CheckForUpdatesAsync()
    {
        var mgr = new UpdateManager(new GithubSource("https://github.com/PlusCosmic/ClipsArchiver.Client.Windows", null, false));

        // check for new version
        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null)
            return; // no update available

        // download new version
        await mgr.DownloadUpdatesAsync(newVersion);

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion);
    }

    private void SaveSettings(FluentWindow? window)
    {
        Settings settings = new()
        {
            UserId = SelectedUser?.Id ?? 0,
            ClipsPath = ClipsPath ?? string.Empty
        };
        SettingsService.SaveSettings(settings);
        window?.Close();
    }
}