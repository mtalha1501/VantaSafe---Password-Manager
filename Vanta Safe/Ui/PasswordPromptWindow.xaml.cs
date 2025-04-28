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
    /// Interaction logic for PasswordPromptWindow.xaml
    /// </summary>
    public partial class PasswordPromptWindow : Window
    {
        public string Password => txtPassword.Password;

        public PasswordPromptWindow()
        {
            InitializeComponent();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    txtError.Text = "Password cannot be empty!";
                    txtError.Visibility = Visibility.Visible;
                    return;
                }

                string currentUser = Application.Current.Properties["CurrentUser"] as string;
                if (AuthService.VerifyMasterPassword(txtPassword.Password, currentUser, out string deviceSecret))
                {
                    // Store verification results temporarily
                    Application.Current.Properties["TempMasterKey"] =
                        AuthService.DeriveMasterKey(txtPassword.Password, deviceSecret);
                    Application.Current.Properties["TempDeviceSecret"] = deviceSecret;

                    this.DialogResult = true;
                }
                else
                {
                    txtError.Text = "Invalid master password!";
                    txtError.Visibility = Visibility.Visible;
                }
            }
            finally
            {
                txtPassword.Password = string.Empty;
                GC.Collect();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
