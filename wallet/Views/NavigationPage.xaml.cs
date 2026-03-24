using Microsoft.UI.Xaml.Controls;
using TryingOurBest1.ViewModels;

namespace TryingOurBest1.Views
{
    public sealed partial class NavigationPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public NavigationPage()
        {
            this.InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Load default page on startup
            NavigateToCurrentView();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CurrentViewModel))
            {
                NavigateToCurrentView();
            }
        }

        private void NavigateToCurrentView()
        {
            if (ViewModel.CurrentViewModel is WalletViewModel walletVM)
            {
                ContentArea.Content = new WalletView(walletVM);
            }
            // TODO: add more as you create them
            // else if (ViewModel.CurrentViewModel is ShopViewModel shopVM)
            //     ContentArea.Content = new ShopView(shopVM);
            // else if (ViewModel.CurrentViewModel is MarketplaceViewModel marketVM)
            //     ContentArea.Content = new MarketplaceView(marketVM);
        }
    }
}