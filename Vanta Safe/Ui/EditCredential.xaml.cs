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
using Vanta_Safe.Models;
using Vanta_Safe.Services;
using Vanta_Safe.Ui;

namespace Vanta_Safe
{
    /// <summary>
    /// Interaction logic for EditCredential.xaml
    /// </summary>
    public partial class EditCredential : Window
    {
        Credential credential;
        public EditCredential(Credential credential, byte[] masterKey)
        {
            this.credential = credential;

            
            InitializeComponent();
            txtSiteUrl.Text = EncryptDecryptService.DecryptSafely(this.credential.EncryptedSiteUrl, masterKey);
            txtSiteName.Text = EncryptDecryptService.DecryptSafely(this.credential.EncryptedSiteName, masterKey);
            txtUsername.Text = EncryptDecryptService.DecryptSafely(this.credential.EncryptedUsername, masterKey);
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSiteName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Site Name, Username, and Password are required!", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Get the current user's MasterKey (stored during login)
                byte[] masterKey = (byte[])Application.Current.Properties["MasterKey"];
                string currentUser = (string)Application.Current.Properties["CurrentUser"];



                // Encryption
                if (masterKey == null)
                {
                    MessageBox.Show("ms key is null");
                }

                byte[] encryptedSiteName = EncryptDecryptService.EncryptField(txtSiteName.Text, masterKey);
                byte[] encryptedUsername = EncryptDecryptService.EncryptField(txtUsername.Text, masterKey);
                byte[] encryptedPassword = EncryptDecryptService.EncryptField(txtPassword.Password, masterKey);
                byte[] encryptedUrl = !string.IsNullOrEmpty(txtSiteUrl.Text) ? EncryptDecryptService.EncryptField(txtSiteUrl.Text, masterKey) : null;


                //Save to database (Step 4)
                if (!SaveCredentialToDb(encryptedSiteName, encryptedUsername,
                   encryptedPassword, encryptedUrl, currentUser))
                {
                    MessageBox.Show($"Failed to save credential: Already Exists ");
                }
                else
                {
                    MessageBox.Show("UPDATED Credential saved successfully!", "Success",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save credential: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private bool SaveCredentialToDb(byte[] encryptedSiteName, byte[] encryptedUsername,
                               byte[] encryptedPassword, byte[] encryptedUrl,
                               string currentUser)
        {
            int credentialId = this.credential.Id;
            using (var connection = new SqliteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();

                //// Check duplicates via decryption
                //if (CredentialExists(connection, currentUser,
                //    (byte[])Application.Current.Properties["MasterKey"],
                //    txtUsername.Text, txtSiteUrl.Text))
                //{
                //    MessageBox.Show("This credential already exists!", "Duplicate",
                //                   MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return false;
                //}

                byte[] masterKey = (byte[])Application.Current.Properties["MasterKey"];
                byte[] usernameHash = AuthService.GenerateHash(txtUsername.Text, masterKey);
                byte[] urlHash = AuthService.GenerateHash(txtSiteUrl.Text, masterKey);

                var cmd = connection.CreateCommand();

                cmd.CommandText = @"
            UPDATE Credentials
SET 
    EncryptedSiteName = @encryptedSiteName,
    EncryptedSiteUrl = @encryptedSiteUrl,
    EncryptedUsername = @encryptedUsername,
    EncryptedPassword = @encryptedPassword,
    UsernameHash = @UsernameHash,
    SiteUrlHash = @SiteUrlHash
WHERE 
    Id = @credentialId; 
            ";

                cmd.Parameters.AddWithValue("@currentUser", currentUser);
                cmd.Parameters.AddWithValue("@encryptedSiteName", encryptedSiteName);
                cmd.Parameters.AddWithValue("@encryptedSiteUrl", encryptedUrl ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@encryptedUsername", encryptedUsername);
                cmd.Parameters.AddWithValue("@encryptedPassword", encryptedPassword);
                cmd.Parameters.AddWithValue("@UsernameHash", usernameHash);
                cmd.Parameters.AddWithValue("@SiteUrlHash", urlHash);
                cmd.Parameters.AddWithValue("@credentialId", this.credential.Id);

                cmd.ExecuteNonQuery();
                return true;
            }
        }
        private bool CredentialExists(SqliteConnection connection, string currentUser,
                            byte[] masterKey, string username, string url)
        {

            byte[] usernameHash = AuthService.GenerateHash(username, masterKey);
            byte[] urlHash = AuthService.GenerateHash(url, masterKey);

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
        SELECT 1 FROM Credentials 
        WHERE UserId = (SELECT Id FROM Users WHERE Username = @currentUser)
        AND UsernameHash = @usernameHash
        AND SiteUrlHash = @urlHash
        LIMIT 1";

            cmd.Parameters.AddWithValue("@currentUser", currentUser);
            cmd.Parameters.AddWithValue("@usernameHash", usernameHash);
            cmd.Parameters.AddWithValue("@urlHash", urlHash);

            cmd.ExecuteNonQuery();

            return cmd.ExecuteScalar() != null;



        }
    }
}
