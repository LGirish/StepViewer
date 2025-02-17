using System.Windows;
using StepAPIService;

namespace HelixViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalService.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LocalService.Stop();
        }
    }

}
