using GalaSoft.MvvmLight.Messaging;
using Stuff;
using System;
using GalaSoft.MvvmLight;
using Windows.ApplicationModel.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Time_Sense.ViewModels
{
    public class AnalysisViewModel : ViewModelBase
    {
        public AnalysisViewModel()
        {
            Messenger.Default.Register<MessageHelper.AnalysisMessage>(this, message =>
            {
                MainPage.title.Text = utilities.loader.GetString("analytics");
                NavigatedTo();
            });
        }

        private bool _checkingLicense = false;
        public bool checkingLicense
        {
            get
            {
                return _checkingLicense;
            }
            set
            {
                Set(ref _checkingLicense, value);
            }
        }

        private bool _loading = false;
        public bool loading
        {
            get
            {
                return _loading;
            }
            set
            {
                Set(ref _loading, value);
            }
        }

        //PIVOT
        private int _paramPivotIndex = 0;
        public int paramPivotIndex
        {
            get
            {
                return _paramPivotIndex;
            }
            set
            {
                Set(ref _paramPivotIndex, value);
            }
        }

        private async void NavigatedTo()
        {
            checkingLicense = true;
            try
            {
                var license = CurrentApp.LicenseInformation;
                if (!license.ProductLicenses["ts_analytics"].IsActive)
                {
                    var result = await new PurchaseDialog().ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        try
                        {
                            //TRY TO CONTACT THE STORE  
                            PurchaseResults p = await CurrentApp.RequestProductPurchaseAsync("ts_analytics");
                            if (p.Status != ProductPurchaseStatus.AlreadyPurchased && p.Status != ProductPurchaseStatus.Succeeded)
                            {
                                MainPage.home.IsChecked = true;
                            }
                            if (!license.ProductLicenses["ts_analytics"].IsActive)
                            {
                                MainPage.home.IsChecked = true;
                            }
                        }
                        catch
                        {
                            await new MessageDialog(utilities.loader.GetString("error_transaction"), utilities.loader.GetString("error")).ShowAsync();
                            MainPage.home.IsChecked = true;
                        }
                    }
                    else
                    {
                        if (utilities.STATS.Values[settings.analysis_trial] == null)
                        {
                            utilities.STATS.Values[settings.analysis_trial] = "tried";
                        }
                        else
                        {
                            MainPage.home.IsChecked = true;
                        }
                    }
                }
            }
            catch
            {
                await new MessageDialog(utilities.loader.GetString("error_transaction_internet"), utilities.loader.GetString("error")).ShowAsync();
                MainPage.home.IsChecked = true;
            }
            checkingLicense = false;
        }
    }
}
