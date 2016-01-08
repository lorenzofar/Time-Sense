using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// Il modello di elemento per il controllo utente è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234236

namespace Time_Sense
{
    public sealed partial class WelcomeDialog : ContentDialog
    {
        bool start = true;

        public WelcomeDialog()
        {
            this.InitializeComponent();
            
        }

        private void flip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!start)
            {
                int a = flip.SelectedIndex;
                for (int i = 0; i < 8; i++)
                {
                    FontIcon f = dots_panel.FindName(String.Format("dot_{0}", i)) as FontIcon;
                    f.Foreground = new SolidColorBrush(Colors.Gray);
                }
                SolidColorBrush brush = Windows.UI.Xaml.Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
                FontIcon d = dots_panel.FindName(String.Format("dot_{0}", a)) as FontIcon;
                d.Foreground = brush;
            }
            else
            {
                start = false;
            }
        }

        private void dot_1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var dot = sender as FontIcon;
            string index = dot.Name.Split('_')[1];
            int i = int.Parse(index);
            flip.SelectedIndex = i;
        }
    }
}
