using Microsoft.UI.Xaml;

namespace MovieMarketplace
{
    public partial class App : Application
    {
        // Ambele sunt acum 'settable' pentru a elimina eroarea CS0200
        public static Window? m_window { get; set; }
        public static Window? MainWindow { get; set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new MainWindow();

            m_window = window;
            MainWindow = window;

            window.Activate();
        }
    }
}