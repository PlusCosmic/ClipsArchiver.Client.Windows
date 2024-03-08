using System.Windows;
using Serilog;
using Velopack;

namespace ClipsArchiver;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try {
            VelopackApp.Build().Run();

            // We can now launch the WPF application as normal.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                              @"\ClipsArchiver\logs\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            var app = new App();
            app.InitializeComponent();
            app.Run();
        } catch (Exception ex) {
            Log.Fatal(ex, "Fatal Exception");
        }
    }
}