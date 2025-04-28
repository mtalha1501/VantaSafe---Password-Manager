using System.Configuration;
using System.Data;
using System.Windows;
using Vanta_Safe.Services;

namespace Vanta_Safe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                DatabaseService.Initialize();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical startup error: {ex.Message}");
                Application.Current.Shutdown();
            }
        }
    }

}
