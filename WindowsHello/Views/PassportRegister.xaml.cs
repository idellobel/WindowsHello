using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsHello.Domain.Services.AuthService;
using WindowsHello.Helpers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsHello.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PassportRegister : Page
    {
        public PassportRegister()
        {
            this.InitializeComponent();
        }

        private async void RegisterButton_Click_Async(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = "";

            //Validate entered credentials are acceptable
            if (!string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                //Register an Account on the AuthService so that we can get back a userId
                AuthService.Instance.Register(UsernameTextBox.Text);
                Guid userId = AuthService.Instance.GetUserId(UsernameTextBox.Text);

                if (userId != Guid.Empty)
                {
                    //Now that the account exists on server try and create the necessary passport details and add them to the account
                    bool isSuccessful = await MicrosoftPassport.CreatePassportKeyAsync(userId, UsernameTextBox.Text);
                    if (isSuccessful)
                    {
                        //Navigate to the Welcome Screen. 
                        Frame.Navigate(typeof(Welcome), AuthService.Instance.GetUserAccount(userId));
                    }
                    else
                    {
                        //The passport account creation failed.
                        //Remove the account from the server as passport details were not configured
                        AuthService.Instance.PassportRemoveUser(userId);

                        ErrorMessage.Text = "Aanmaak account mislukt!";
                    }
                }
            }
            else
            {
                ErrorMessage.Text = "Vul een gebruikersnaam in!";
            }

        }
    }
}
