using ClipsArchiver.ViewModels;
using Wpf.Ui.Controls;

namespace ClipsArchiver.Windows;

public partial class UploadWindow : FluentWindow
{
    public UploadWindow()
    {
        InitializeComponent();
        DataContext = new UploadViewModel();
    }
}