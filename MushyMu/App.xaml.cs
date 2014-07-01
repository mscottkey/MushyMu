using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Media;



namespace MushyMu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        
        static App()
        {
            DispatcherHelper.Initialize();
        }

        //private void StartupHandler(object sender, System.Windows.StartupEventArgs e)
        //{
        //    Elysium.Manager.Apply(this, Elysium.Theme.Dark, Brushes.DarkRed, Brushes.Black);
        //}
    }
}
