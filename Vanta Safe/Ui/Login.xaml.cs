using Microsoft.Data.Sqlite;
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
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var cmdFetch = connection.CreateCommand();

                // Failed Attempt handle
                cmdFetch.CommandText  = @"SELECT FailedAttempts FROM Users WHERE Username = @username";
                cmdFetch.Parameters.AddWithValue("@username", txtUsername.Text);

                var result = cmdFetch.ExecuteScalar();
                if (result == null)
                {
                    txtError.Text = "Invalid username or password!";
                    txtError.Visibility = Visibility.Visible;
                    return;
                }

                int failedAttempts = Convert.ToInt32(result);

                // If already locked out
                if (failedAttempts >= 5)
                {
                    txtError.Text = "Account locked due to multiple failed attempts!";
                    MessageBox.Show("Account has been Locked out for invalid attempts");
                    txtError.Visibility = Visibility.Visible;
                    Application.Current.Shutdown();  // Close the app
                    return;
                }
                connection.Close();
            }


            if (AuthService.VerifyMasterPassword(txtPassword.Password, txtUsername.Text, out var deviceSecret))
            {
                var masterKey = AuthService.DeriveMasterKey(txtPassword.Password, deviceSecret);
                App.Current.Properties["MasterKey"] = masterKey;
                App.Current.Properties["CurrentUser"] = txtUsername.Text;

                new VaultWindow().Show();
                this.Close();
            }
            else
            {
                using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
                {
                    connection.Open();
                    var cmdFetch = connection.CreateCommand();
                    cmdFetch.CommandText = @"UPDATE Users SET FailedAttempts = FailedAttempts + 1 WHERE Username = @username";
                    cmdFetch.Parameters.AddWithValue("@username", txtUsername.Text);
                    cmdFetch.ExecuteNonQuery();
                    connection.Close();
                }

                txtError.Text = "Invalid username or password!";
                txtError.Visibility = Visibility.Visible;
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

        private void LinkRecovery_Click(object sender, RoutedEventArgs e)
        {
            var v = new RecoveryWindow();
            v.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }
    }
}
