using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System;
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
                //MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory);
                DatabaseService.Initialize();
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical startup error: {ex.Message}");
                File.WriteAllText("crashlog.txt", ex.ToString());
                MessageBox.Show("Startup failed. Check errorlog.txt.");
                Application.Current.Shutdown();
            }
        }
    }

}
