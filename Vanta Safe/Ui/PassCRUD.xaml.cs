using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using Vanta_Safe.Models;
using Vanta_Safe.Services;

namespace Vanta_Safe.Ui
{
    /// <summary>
    /// Interaction logic for PassCRUD.xaml
    /// </summary>
    public partial class PassCRUD : Window
    {
        private Credential credential;
        private byte[] masterKey;
        public PassCRUD(Credential credential, byte[] masterKey)
        {
            this.credential = credential;
            this.masterKey = masterKey;
            InitializeComponent();
            txtUrl.Text = EncryptDecryptService.DecryptSafely(credential.EncryptedSiteUrl, masterKey);
            txtSite.Text = EncryptDecryptService.DecryptSafely(credential.EncryptedSiteName, masterKey);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Window v = new EditCredential(this.credential);
            v.Show();
            this.Close();

        }

        private void CopyPass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pass = $"{ EncryptDecryptService.DecryptSafely(credential.EncryptedPassword, masterKey)}";
                Clipboard.SetText(pass);


                // Auto-clear after 15 seconds
                Task.Delay(TimeSpan.FromSeconds(15)).ContinueWith(_ =>
                {
                    if (Clipboard.GetText() == pass)
                    {
                        Clipboard.Clear();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                btnCopyPass.Content = "✓ Copied!";



                Task.Delay(2000).ContinueWith(_ =>
                {
                    //btnCopyPass.Content = "Copy Password";
                }, TaskScheduler.FromCurrentSynchronizationContext());


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy: {ex.Message}");
            }
        }

        private void btnCopyAll_clcik(object sender, RoutedEventArgs e)
        {
            try
            {
                string pass = $"Site Name: {EncryptDecryptService.DecryptSafely(credential.EncryptedSiteName, masterKey)}\n" +
                    $"Site URL: {EncryptDecryptService.DecryptSafely(credential.EncryptedSiteUrl, masterKey)}\n"+
                    $"Password: {EncryptDecryptService.DecryptSafely(credential.EncryptedPassword, masterKey)}\n" +
                    $"Username or Email: {EncryptDecryptService.DecryptSafely(credential.EncryptedUsername, masterKey)}";
                Clipboard.SetText(pass);


                // Auto-clear after 15 seconds
                Task.Delay(TimeSpan.FromSeconds(15)).ContinueWith(_ =>
                {
                    if (Clipboard.GetText() == pass)
                    {
                        Clipboard.Clear();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                btnCopyAll.Content = "✓ Copied!";



                Task.Delay(2000).ContinueWith(_ =>
                {
                   // btnCopyAll.Content = "Copy All Details";
                }, TaskScheduler.FromCurrentSynchronizationContext());


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Credentials WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", credential.Id);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 1)
                    {
                        MessageBox.Show("Credential deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Error: Credential not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting credential: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
