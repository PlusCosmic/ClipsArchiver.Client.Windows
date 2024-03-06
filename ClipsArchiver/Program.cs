using System.Windows;
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
            var app = new App();
            app.InitializeComponent();
            app.Run();
        } catch (Exception ex) {
            MessageBox.Show("Unhandled exception: " + ex.ToString());
        }
    }
}