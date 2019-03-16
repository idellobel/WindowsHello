using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WindowsHello.Domain.Models;
using WindowsHello.Domain.Services.AuthService;
using WindowsHello.Helpers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsHello.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Welcome : Page
    {
        private User _activeAccount;


        public Welcome()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _activeAccount = (User)e.Parameter;
            if (_activeAccount != null)
            {
                User account = AuthService.Instance.GetUserAccount(_activeAccount.UserId);
                if (account != null)
                {
                    UserListView.ItemsSource = account.PassportDevices;
                    UserNameText.Text = account.Username;
                }
            }
        }

        private void Button_Restart_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserSelection));
        }



        private void Button_Forget_Device_Click(object sender, RoutedEventArgs e)
        {
            PassportDevice selectedDevice = UserListView.SelectedItem as PassportDevice;
            if (selectedDevice != null)
            {
                //Remove it from Windows Hello
                MicrosoftPassport.RemovePassportDevice(_activeAccount, selectedDevice.DeviceId);

                Debug.WriteLine("User " + _activeAccount.Username + " deleted.");

                if (!UserListView.Items.Any())
                {
                    //Navigate back to UserSelection page.
                    Frame.Navigate(typeof(UserSelection));
                }
            }
            else
            {
                ForgetDeviceErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void ForgetButton_Click(object sender, RoutedEventArgs e)
        {
            //Remove it from Windows Hello
            MicrosoftPassport.RemovePassportAccountAsync(_activeAccount);

            Debug.WriteLine("User " + _activeAccount.Username + " deleted.");

            //Navigate back to UserSelection page.
            Frame.Navigate(typeof(UserSelection));
        }
    }

}
