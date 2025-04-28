using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Vanta_Safe.Ui
{
    /// <summary>
    /// Interaction logic for VaultWindow.xaml
    /// </summary>
    public partial class VaultWindow : Window
    {
        public VaultWindow()
        {
            InitializeComponent();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Window main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void BtnPasswords_Click(object sender, RoutedEventArgs e)
        {
            Window pass = new PasswordsWindow();
            pass.Show();
            this.Close();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var p = new HelpWindow();
            p.Show();
            this.Close();
        }
    }
}
