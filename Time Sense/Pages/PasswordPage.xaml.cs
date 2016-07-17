using System;
using System.Threading.Tasks;
using Stuff;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Credentials.UI;

namespace Time_Sense
{
    public sealed partial class PasswordPage : Page
    {
        string pass = "";

        public PasswordPage()
        {
            this.InitializeComponent();
        }             

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e != null)
            {
                pass = e.Parameter.ToString();
            }
            keyboard_grid.Visibility = utilities.STATS.Values[settings.password] == null ? Visibility.Collapsed : Visibility.Visible;
            hello_btn.Visibility = utilities.STATS.Values[settings.windows_hello] == null ? Visibility.Collapsed : Visibility.Visible;
            if (utilities.STATS.Values[settings.windows_hello] != null)
            {
                AuthenticateUser();
            }
        }

        private async void AuthenticateUser()
        {
            UserConsentVerificationResult consentResult = await UserConsentVerifier.RequestVerificationAsync(utilities.loader.GetString("hello_message"));
            switch (consentResult)
            {
                case UserConsentVerificationResult.Verified:
                    Login();
                    break;
            }
        }

        private void go_btn_Click(object sender, RoutedEventArgs e)
        {
            if (password_box.Password == pass)
            {
                Login();
            }
            else
            {
                password_box.Password = password_box.Password.Remove(0);
                password_box.PlaceholderText = utilities.loader.GetString("password_wrong");
                error.Begin();
            }
        }

        private async void Login()
        {
            this.lock_ellipse.Fill = new SolidColorBrush(Colors.Green);
            this.lock_icon.Glyph = "";
            this.lock_panel.Visibility = Visibility.Visible;
            this.keyboard_grid.Visibility = Visibility.Collapsed;
            this.hello_btn.Visibility = Visibility.Collapsed;
            await Task.Delay(5);
            Frame.Navigate(typeof(MainPage), App.jump_arguments);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            password_box.Password += btn.Content.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //ELIMINA TUTTE
            password_box.Password = "";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //ELIMINA 1
            try {
                string p = password_box.Password.ToString().Remove(password_box.Password.ToString().Length - 1);
                password_box.Password = p;
            } catch { }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Number0:
                    password_box.Password += "0";
                    break;
                case Windows.System.VirtualKey.Number1:
                    password_box.Password += "1";
                    break;
                case Windows.System.VirtualKey.Number2:
                    password_box.Password += "2";
                    break;
                case Windows.System.VirtualKey.Number3:
                    password_box.Password += "3";
                    break;
                case Windows.System.VirtualKey.Number4:
                    password_box.Password += "4";
                    break;
                case Windows.System.VirtualKey.Number5:
                    password_box.Password += "5";
                    break;
                case Windows.System.VirtualKey.Number6:
                    password_box.Password += "6";
                    break;
                case Windows.System.VirtualKey.Number7:
                    password_box.Password += "7";
                    break;
                case Windows.System.VirtualKey.Number8:
                    password_box.Password += "8";
                    break;
                case Windows.System.VirtualKey.Number9:
                    password_box.Password += "9";
                    break;
                case Windows.System.VirtualKey.Back:
                    try {
                        string p = password_box.Password.ToString().Remove(password_box.Password.ToString().Length - 1);
                        password_box.Password = p;
                    }
                    catch { }
                    break;
                case Windows.System.VirtualKey.Escape:
                    password_box.Password = "";
                    break;
                case Windows.System.VirtualKey.Enter:
                    go_btn_Click(go_btn, null);
                    break;
            }
        }

        private void hello_btn_Click(object sender, RoutedEventArgs e)
        {
            AuthenticateUser();
        }
    }
}
