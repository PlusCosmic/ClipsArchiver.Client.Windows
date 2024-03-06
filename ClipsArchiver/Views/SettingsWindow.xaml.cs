using System.Windows;
using System.Windows.Input;
using ClipsArchiver.ViewModels;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ClipsArchiver.Views;

public partial class SettingsWindow : FluentWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }

    private void FolderIconPressed(object sender, MouseButtonEventArgs e)
    {
        OpenFolderDialog folderDialog = new();
        if (folderDialog.ShowDialog() ?? false)
        {
            if (DataContext is not SettingsViewModel viewModel)
            {
                return;
            }

            viewModel.ClipsPath = folderDialog.FolderName;
        }
    }

    private void CloseButtonPressed(object sender, RoutedEventArgs e)
    {
        Close();
    }
}