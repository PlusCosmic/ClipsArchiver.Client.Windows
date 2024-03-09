using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClipsArchiver.Windows;

namespace ClipsArchiver.Views;

public partial class ClipPanelView : UserControl
{
    public ClipPanelView()
    {
        InitializeComponent();
    }

    private void SettingsIconPressed(object sender, MouseButtonEventArgs e)
    {
        SettingsWindow window = new();
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        window.Show();
    }

    private void UploadIconPressed(object sender, MouseButtonEventArgs e)
    {
        UploadWindow window = new();
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        window.Show();
    }
}