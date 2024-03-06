using System.Windows.Controls;
using System.Windows.Input;
using ClipsArchiver.Models;

namespace ClipsArchiver.Controls;

public partial class ClipCard : UserControl
{
    public ClipCard()
    {
        InitializeComponent();
    }

    private void PlayIconPressed(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not ClipModel model)
        {
            return;
        }

        if (model.ShowVideoCommand.CanExecute(model))
        {
            model.ShowVideoCommand.Execute(model);
        }
    }
}