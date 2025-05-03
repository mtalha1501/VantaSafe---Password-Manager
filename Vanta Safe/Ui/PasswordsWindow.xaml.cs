using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for PasswordsWindow.xaml
    /// </summary>
    public partial class PasswordsWindow : Window
    {
        public PasswordsWindow()
        {
            InitializeComponent();
            LoadCredentials();  // Add this line

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Window Rg = new VaultWindow();
            Rg.Show();
            this.Close();
        }
        private void PasswordSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems.Count == 0)
                return;
           
        }
       
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                GoBack_Click(null, null);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Call your search method here
                SearchPasswords(txtSearch.Text);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Open Add Credential window
            new AddCredentialWindow().Show();
            this.Close();
        }
        private void SearchPasswords(string text)
        {
            byte[] masterKey = (byte[])Application.Current.Properties["MasterKey"];
            string currentUser = (string)Application.Current.Properties["CurrentUser"];

            lstPasswords.Items.Clear();

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
            SELECT EncryptedSiteName, EncryptedSiteUrl, EncryptedUsername 
            FROM Credentials 
            WHERE UserId = (SELECT Id FROM Users WHERE Username = @username)";
                cmd.Parameters.AddWithValue("@username", currentUser);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string siteName = EncryptDecryptService.DecryptSafely((byte[])reader["EncryptedSiteName"], masterKey);
                        string url = EncryptDecryptService.DecryptSafely((byte[])reader["EncryptedSiteUrl"], masterKey);
                        string username = EncryptDecryptService.DecryptSafely((byte[])reader["EncryptedUsername"], masterKey);

                        if (siteName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                            url.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                            username.Contains(text, StringComparison.OrdinalIgnoreCase))
                        {
                            lstPasswords.Items.Add($"{siteName}\nlink: {url}");
                        }
                    }
                }
            }
        }




        private void LoadCredentials()
        {
            byte[] masterKey = (byte[])Application.Current.Properties["MasterKey"];
            string currentUser = (string)Application.Current.Properties["CurrentUser"];

            lstPasswords.Items.Clear();

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
            SELECT EncryptedSiteName, EncryptedSiteUrl 
            FROM Credentials 
            WHERE UserId = (SELECT Id FROM Users WHERE Username = @username)";
                cmd.Parameters.AddWithValue("@username", currentUser);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Decrypt only SiteName + URL for the list
                        string siteName = EncryptDecryptService.DecryptSafely((byte[])reader["EncryptedSiteName"], masterKey);
                        string url = EncryptDecryptService.DecryptSafely((byte[])reader["EncryptedSiteUrl"], masterKey);

                        lstPasswords.Items.Add($"{siteName}\nlink: {url}");
                    }
                }
            }
        }

        private void OnCredentialDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. Get displayed site name from ListBox
            var selectedItem = lstPasswords.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedItem)) return;

            string siteName = selectedItem.Split('\n')[0].Trim();

            // 2. Find credential by decrypted site name (secure approach)
            byte[] masterKey = (byte[])Application.Current.Properties["MasterKey"];
            string currentUser = (string)Application.Current.Properties["CurrentUser"];

            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
            SELECT * FROM Credentials 
            WHERE UserId = (SELECT Id FROM Users WHERE Username = @currentUser)";

                cmd.Parameters.AddWithValue("@currentUser", currentUser);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 3. Compare decrypted site names
                        byte[] encryptedSiteName = (byte[])reader["EncryptedSiteName"];
                        string storedSiteName = EncryptDecryptService.DecryptField(encryptedSiteName, masterKey);

                        if (storedSiteName.Equals(siteName, StringComparison.OrdinalIgnoreCase))
                        {
                            var credential = new Credential
                            {
                                Id = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                EncryptedSiteName = (byte[])reader["EncryptedSiteName"],
                                EncryptedSiteUrl = (byte[])reader["EncryptedSiteUrl"],
                                EncryptedUsername = (byte[])reader["EncryptedUsername"],
                                EncryptedPassword = (byte[])reader["EncryptedPassword"]
                            };

                            // Proceed with password prompt...
                            var passwordWindow = new PasswordPromptWindow();
                            if (passwordWindow.ShowDialog() == true)
                            {

                                masterKey = (byte[])Application.Current.Properties["MasterKey"];
                                string deviceSecret = (string)Application.Current.Properties["DeviceSecret"];
                                Window detailWindow = new PassCRUD(credential, masterKey);
                                detailWindow.Show();
                                btnRefresh_Click(sender, e);
                                // Immediately clear temporary storage
                                //Application.Current.Properties.Remove("TempMasterKey");
                                //Application.Current.Properties.Remove("TempDeviceSecret");
                            }
                            return;
                        }
                    }
                }
            }
            MessageBox.Show("Credential not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            LoadCredentials(); // Your existing method to reload everything
        }

    }
}
