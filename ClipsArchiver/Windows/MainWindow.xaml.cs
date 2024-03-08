using ClipsArchiver.ViewModels;
using Wpf.Ui.Controls;

namespace ClipsArchiver.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}