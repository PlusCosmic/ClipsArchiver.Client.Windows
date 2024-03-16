using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClipsArchiver.ViewModels;

namespace ClipsArchiver.Views;

public partial class ClipVideoView : UserControl
{
    public ClipVideoView()
    {
        InitializeComponent();
    }
    
    private void BackIconPressed(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }
        viewModel.GoBackCommand.Execute(null);
    }

    private void SliderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }
        viewModel.IsScrubbing = true;
    }

    private void SliderMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }
        viewModel.IsScrubbing = false;
    }

    private void ClipVideoView_OnKeyDown(object sender, KeyEventArgs e)
    {
    }
}