using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        window.Show();
    }
}