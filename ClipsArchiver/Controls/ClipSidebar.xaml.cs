using System.Windows.Controls;
using System.Windows.Input;
using ClipsArchiver.ViewModels;

namespace ClipsArchiver.Controls;

public partial class ClipSidebar : UserControl
{
    public ClipSidebar()
    {
        InitializeComponent();
    }

    private void AutoSuggestBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }
        
        AddTag();
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        AddTag();
    }

    private void AddTag()
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }
        viewModel.AddTagToSelectedClipCommand.Execute(AutoSuggestBox.Text);
        AutoSuggestBox.Text = string.Empty;
        AutoSuggestBox.Focus();
    }
}