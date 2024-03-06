using System.Windows;
using Velopack;
using Velopack.Sources;

namespace ClipsArchiver;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        VelopackApp.Build().Run();
    }
}