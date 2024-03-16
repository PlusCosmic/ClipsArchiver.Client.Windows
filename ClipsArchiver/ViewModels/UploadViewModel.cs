using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using ClipsArchiver.Entities;
using ClipsArchiver.Models;
using ClipsArchiver.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ClipsArchiver.ViewModels;

public class UploadViewModel : ViewModelBase
{
    private ObservableCollection<UnsavedClipModel> _unsavedClipModels;
    public ObservableCollection<UnsavedClipModel> UnsavedClipModels
    {
        get => _unsavedClipModels;
        set => SetField(ref _unsavedClipModels, value);
    }

    public RelayCommand OpenFileDialogCommand { get; private set; }
    public AsyncRelayCommand UploadClipsCommand { get; private set; }
    public RelayCommand<FluentWindow> CloseWindowCommand { get; private set; }
    
    public UploadViewModel()
    {
        _unsavedClipModels = new ObservableCollection<UnsavedClipModel>();
        OpenFileDialogCommand = new RelayCommand(OpenFileDialog);
        UploadClipsCommand = new AsyncRelayCommand(UploadClips);
        CloseWindowCommand = new RelayCommand<FluentWindow>(CloseWindow);
    }

    private void OpenFileDialog()
    {
        var settings = SettingsService.GetSettings();
        var fileDialog = new OpenFileDialog();
        fileDialog.DefaultDirectory = settings.ClipsPath;
        fileDialog.Multiselect = true;
        fileDialog.Filter = "mp4 files (*.mp4)|*.mp4";
        if (fileDialog.ShowDialog() ?? false)
        {
            foreach (var filename in fileDialog.FileNames)
            {
                UnsavedClipModels.Add(new UnsavedClipModel(filename));
            }
        }
    }

    private async Task UploadClips()
    {
        List<Task> tasks = new();
        foreach (var unsavedClipModel in UnsavedClipModels)
        {
            tasks.Add(unsavedClipModel.UploadClipAsync());
        }

        await Task.WhenAll(tasks);
    }

    private void CloseWindow(FluentWindow? window)
    {
        window?.Close();
    }

    public async Task AddNewClipAndUploadAsync(string filename)
    {
        UnsavedClipModel model = new(filename);
        LocalDbService.SetInfoForFileName(new LocalClipInfo{ FileName = Path.GetFileName(filename), Watched = false, ClipId = -1 });
        Application.Current.Dispatcher.Invoke(() => UnsavedClipModels.Add(model));
        await model.UploadClipAsync(true);
    }
}