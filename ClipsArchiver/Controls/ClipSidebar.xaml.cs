using System.Windows.Controls;
using System.Windows.Input;
using ClipsArchiver.ViewModels;
using Wpf.Ui.Controls;

namespace ClipsArchiver.Controls;

public partial class ClipSidebar : UserControl
{
    public ClipSidebar()
    {
        InitializeComponent();
    }

    private void OnTagSelectionSubmitted()
    {
        AutoSuggestBox.Text = string.Empty;
    }

    private void AutoSuggestBox_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnTagSelectionSubmitted();
        }
    }

    private void SearchIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        OnTagSelectionSubmitted();
    }

    private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }
        viewModel.AddTagToSelectedClipCommand.Execute(args.SelectedItem);
        args.Handled = true;
        OnTagSelectionSubmitted();
        
    }
}