using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
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
    /// Interaction logic for RecoveryWindow.xaml
    /// </summary>
    public partial class RecoveryWindow : Window
    {
        public RecoveryWindow()
        {
            InitializeComponent();
        }

        private void SecretKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RecoverButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string secretKey = SecretKeyTextBox.Text.Trim();


            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(secretKey))
            {
                StatusMessage.Text = "Please fill all fields.";
                return;
            }

            bool recoverySuccessful = TryRecoverAccount(username, secretKey);

            byte[] aesKey = AuthService.DeriveKeyFromDeviceSecretOnly(secretKey);

            if (recoverySuccessful)
            {
                byte[] EncryptedKey; 
                using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                                SELECT EncryptedMasterKey 
                                FROM Users 
                                WHERE Username = @username AND DeviceSecret = @secretKey";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@secretKey", secretKey);
                    EncryptedKey = (byte[])command.ExecuteScalar();

                }
                var masterPassword = EncryptDecryptService.DecryptField(EncryptedKey, aesKey);
                MessageBox.Show("Account successfully recovered!\n Please Remember Your Master Key: "+masterPassword+"\nRegards Vanta Safe Team", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                StatusMessage.Text = "Invalid Username or Secret Key.";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private bool TryRecoverAccount(string username, string secretKey)
        {
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @username AND DeviceSecret = @secretKey";
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@secretKey", secretKey);

                var count = (long)command.ExecuteScalar();

                if (count > 0)
                {
                    var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = "UPDATE Users SET FailedAttempts = 0 WHERE Username = @username";
                    updateCommand.Parameters.AddWithValue("@username", username);
                    updateCommand.ExecuteNonQuery();
                    return true;
                }
            }
            return false;
        }

    }
}
