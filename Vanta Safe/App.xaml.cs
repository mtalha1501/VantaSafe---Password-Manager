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
            base.OnStartup(e);

            try
            {
                // Initialize database and services
                MessageBox.Show($"DB will be created at: {DatabaseService.ConnectionString}");
                DatabaseService.Initialize();

                // Show main window
                //new MainWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize application: {ex.Message}",
                              "Critical Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                Shutdown();
            }
        }
    }

}
