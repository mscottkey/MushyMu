using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;

namespace MushyMu.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private ViewModelBase _gameContainer;
        readonly static NewGameViewModel _newGameViewModel = new NewGameViewModel();
        readonly static GameContainerViewModel _gameContainerViewModel = new GameContainerViewModel();
        /// <summary>
        /// Gets the CurrentViewModel
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public ViewModelBase GameContainerViewModel
        {
            get
            {
                return _gameContainer;
            }
            set
            {
                if (_gameContainer == value)
                    return;
                _gameContainer = value;
                RaisePropertyChanged("GameContainerViewModel");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

            _currentViewModel = MainViewModel._newGameViewModel;
            _gameContainer = MainViewModel._gameContainerViewModel;
            Messenger.Default.Register<int>(this, ChangeTab);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                RaisePropertyChanged("SelectedTabIndex");
            }
        }

        public void ChangeTab(int i)
        {
            SelectedTabIndex = i; //Bound property in .xaml
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}