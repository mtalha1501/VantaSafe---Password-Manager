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
using Vanta_Safe.Services;

namespace Vanta_Safe.Ui
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtError.Text = "Username is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            if (AuthService.VerifyUser(txtUsername.Text, txtPassword.Password, out var deviceSecret))
            {
                var masterKey = AuthService.DeriveMasterKey(txtPassword.Password, deviceSecret);
                App.Current.Properties["MasterKey"] = masterKey;
                App.Current.Properties["CurrentUser"] = txtUsername.Text;

                new VaultWindow().Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Invalid username or password!";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void LinkRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().Show();
            this.Close();
        }
    }
}
