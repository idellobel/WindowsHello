using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using WindowsHello.Domain.Models;
using WindowsHello.Helpers;
using WindowsHello.Domain.Services.AuthService;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsHello.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        private User _account;
        private bool _isExistingAccount;

        public Login()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Check Windows Hello is setup and available on this machine
            if (await MicrosoftPassport.MicrosoftPassportAvailableCheckAsync())
            {
                if (e.Parameter != null)
                {
                    _isExistingAccount = true;
                    // Set the account to the existing account being passed in
                    _account = (User)e.Parameter;
                    UsernameTextBox.Text = _account.Username;
                    SignInPassportAsync();
                }
            }
            else
            {
                //Windows Hello not setup so inform the user
                PassportStatus.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 50, 170, 207));
                PassportStatusText.Text = "Microsoft Passport is not setup!\n" +
                    "Please go to Windows Settings and set up a PIN to use it.";
                PassportSignInButton.IsEnabled = false;
            }
        }

        private void PassportSignInButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "";
            SignInPassportAsync();
        }


        private void RegisterButtonTextBlock_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ErrorMessage.Text = "";
            Frame.Navigate(typeof(PassportRegister));
        }

        private async void SignInPassportAsync()
        {
            if (_isExistingAccount)
            {
                if (await MicrosoftPassport.GetPassportAuthenticationMessageAsync(_account))
                {
                    Frame.Navigate(typeof(Welcome), _account);
                }
            }

            else if (AuthService.Instance.ValidateCredentials(UsernameTextBox.Text, PasswordBox.Password))
            {
                Guid userId = AuthService.Instance.GetUserId(UsernameTextBox.Text);

                if (userId != Guid.Empty)
                {
                    //Now that the account exists on server try and create the necessary passport details and add them to the account
                    bool isSuccessful = await MicrosoftPassport.CreatePassportKeyAsync(userId, UsernameTextBox.Text);
                    if (isSuccessful)
                    {
                        Debug.WriteLine("Successfully signed in with Windows Hello!");
                        //Navigate to the Welcome Screen. 
                        _account = AuthService.Instance.GetUserAccount(userId);
                        Frame.Navigate(typeof(Welcome), _account);
                    }
                    else
                    {
                        //The passport account creation failed.
                        //Remove the account from the server as passport details were not configured
                        AuthService.Instance.PassportRemoveUser(userId);

                        ErrorMessage.Text = "Account aanmaken mislukt!";
                    }
                }
            }
            else
            {
                ErrorMessage.Text = "Ongeldige inloggegevens";
            }
        }
    }
}
