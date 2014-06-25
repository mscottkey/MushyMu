using System.Windows;
using MushyMu.ViewModel;

namespace MushyMu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Elysium.Controls.Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}