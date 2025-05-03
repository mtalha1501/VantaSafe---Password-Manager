using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtError.Text = "Username is required!";
                txtError.Visibility = Visibility.Visible;
                return;
            }
            // Only allow a-z, A-Z, 0-9, _, ., @
            if (!Regex.IsMatch(txtUsername.Text, @"^[a-zA-Z0-9_.@]+$"))
            {
                txtError.Text = "Username can only contain letters, numbers, '_', '.' and '@'";
                txtError.Visibility = Visibility.Visible;
                return;
            }
            // Disallow usernames with only numbers
            if (Regex.IsMatch(txtUsername.Text, @"^\d+$"))
            {
                txtError.Text = "Username cannot be numbers only. use letters as well";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            // Validate password complexity
            var (isValid, errorMessage) = PasswordValidator.Validate(txtPassword.Password);
            if (!isValid)
            {
                txtError.Text = errorMessage;
                txtError.Visibility = Visibility.Visible;
                return;
            }

            // Verify password match
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                txtError.Text = "Passwords do not match!";
                txtError.Visibility = Visibility.Visible;
                return;
            }

            // Proceed with registration
            if (AuthService.RegisterUser(txtUsername.Text, txtPassword.Password, out var deviceSecret))
            {
                MessageBox.Show($"Registration successful!\n\n" +
                               $"Your Device Secret:\n{deviceSecret}\n\n" +
                               "Store this securely - you'll need it for account recovery.",
                               "Success",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                new Login().Show();
                this.Close();
            }
            else
            {
                txtError.Text = "Username already exists!";
                txtError.Visibility = Visibility.Visible;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void LinkLogin_Click(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var password = ((PasswordBox)sender).Password;

            // Update requirements
            reqLength.IsChecked = password.Length >= 12;
            reqNumber.IsChecked = Regex.IsMatch(password, @"[0-9]");
            reqCapital.IsChecked = Regex.IsMatch(password, @"[A-Z]");
            reqSpecial.IsChecked = Regex.IsMatch(password, @"[\W_]"); // Matches any non-word char

            // Visual feedback
            reqLength.Foreground = password.Length >= 12 ? Brushes.LimeGreen : Brushes.Gray;
            reqNumber.Foreground = Regex.IsMatch(password, @"[0-9]") ? Brushes.LimeGreen : Brushes.Gray;
            reqCapital.Foreground = Regex.IsMatch(password, @"[A-Z]") ? Brushes.LimeGreen : Brushes.Gray;
            reqSpecial.Foreground = Regex.IsMatch(password, @"[\W_]") ? Brushes.LimeGreen : Brushes.Gray;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnRegister_Click(sender, e);
            }
        }
    }
}
