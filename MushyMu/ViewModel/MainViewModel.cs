using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using System;
using System.Collections.ObjectModel;

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
        private bool _settingsFlyoutState;
        private ViewModelBase _currentViewModel;
        private ViewModelBase _gameContainer;
        readonly static NewGameViewModel _newGameViewModel = new NewGameViewModel();
        readonly static GameContainerViewModel _gameContainerViewModel = new GameContainerViewModel();
        public RelayCommand OpenFlyoutPanelCommand { get; private set; }
       
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


        private ObservableCollection<Game> _gameList = new ObservableCollection<Game>();
        public ObservableCollection<Game> GameList
        {
            get
            {
                if (_gameList == null)
                {
                    _gameList = new ObservableCollection<Game>();
                }
                return _gameList;
            }
        }
        private ObservableCollection<GameViewModel> _gameVMList = new ObservableCollection<GameViewModel>();
        public ObservableCollection<GameViewModel> GameVMList
        {
            get
            {
                if (_gameVMList == null)
                {
                    _gameVMList = new ObservableCollection<GameViewModel>();
                }
                return _gameVMList;
            }
        }

        private ObservableCollection<MuCommand> _gameCommands = new CommandList();
        public ObservableCollection<MuCommand> GameCommands
        {
            get
            {
                if (_gameCommands == null)
                {
                    _gameCommands = new CommandList();
                }
                return _gameCommands;
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
            Messenger.Default.Register<Game>(this, "StartGame", (_game) => ReceiveNewGame(_game));
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                 switch (message.Notification)
                   {
                     case "OpenGameListFlyOut":
                         GameListFlyOut(true);
                         break;

                     case "CommonCmdsFlyOut":
                         CommonCmdsFlyOut(true);
                         break;

        
                     default:
                         break;
 
                    }
            });
            _isGamesEnabled = false;
            OpenFlyoutPanelCommand = new RelayCommand(() => ExecuteOpenFlyoutPanel());
        }

        public void GameListFlyOut(bool value)
        {
            _gamesListFlyOutState = true;
            RaisePropertyChanged("GamesListFlyOutState");
        }

        public void CommonCmdsFlyOut(bool value)
        {
            _commonCmdsFlyOutState = true;
            RaisePropertyChanged("CommonCmdsFlyOutState");
        }
        public void ExecuteOpenFlyoutPanel()
        {
            _settingsFlyoutState = true;
            RaisePropertyChanged("SettingsFlyoutState");
        }
        

        private bool _commonCmdsFlyOutState;
        public bool CommonCmdsFlyOutState
        {
            get
            {
                return _commonCmdsFlyOutState;
            }
            set
            {
                _commonCmdsFlyOutState = value;
                RaisePropertyChanged("CommonCmdsFlyOutState");
            }
        }

        

        private bool _gamesListFlyOutState;
        public bool GamesListFlyOutState
        {
            get
            {
                return _gamesListFlyOutState;
            }
            set
            {
                _gamesListFlyOutState = value;
                RaisePropertyChanged("GamesListFlyOutState");
            }
        }

        public bool SettingsFlyoutState
        {
            get
            {
                return _settingsFlyoutState;
            }
            set
            {
                _settingsFlyoutState = value;
                RaisePropertyChanged("SettingsFlyoutState");
            }
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

        private bool _isGamesEnabled;
        public bool IsGamesEnabled
        {
            get
            {
                return _isGamesEnabled;
            }
            set
            {
                _isGamesEnabled = value;
                RaisePropertyChanged("IsGamesEnabled");
            }
        }

        public void ChangeTab(int i)
        {
            SelectedTabIndex = i; //Bound property in .xaml
            _isGamesEnabled = true; // Enables the Games Tab which is locked by default.
            RaisePropertyChanged("IsGamesEnabled");
        }

        public string Token;

        GameViewModel _currentGame;
        GameViewModel _prevGame;

        private object ReceiveNewGame(Game _game)
        {
            //Set previous game ViewModel to non Active state so it knows it is in the background
            if (_currentGame != null)
            { 
                _prevGame = _currentGame;
                _prevGame.ActiveGame = false;
                _prevGame.HasNewMessage = false;
            }
            

            //Send out game info, then generate the View Model
            Token = Guid.NewGuid().ToString();
            
            _currentGame = new GameViewModel(Token);

            Messenger.Default.Send<Game>(_game, Token);
            RaisePropertyChanged("CurrentGame");
            
            // Add this game to the List
            //_gameList.Add(_game);
            //RaisePropertyChanged("GameList");
            _gameVMList.Add(_currentGame);
            RaisePropertyChanged("GameVMList");
            _selectedGame = GameVMList.Count - 1;
            RaisePropertyChanged("SelectedGame");
            RaisePropertyChanged("CurrentGame");

            // Send out messages to generate the game information and then switch tabs.

            Messenger.Default.Send<int, MainViewModel>(1);
            return null;
        }

        private int _selectedGame;
        public int SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                if (value == _selectedGame)
                    return;
                _selectedGame = value;
                RaisePropertyChanged("SelectedGame");

                //Before selection changes, register previous game
                _prevGame = _currentGame;
               

                //Selection changed, switch view to that VM
                _currentGame = GetSelectedGameByID(_selectedGame);
                RaisePropertyChanged("CurrentGame");

                //Set current game as ActiveGame
                _currentGame.ActiveGame = true;
                _currentGame.HasNewMessage = false;

                //Close the flyout
                _gamesListFlyOutState = false;
                RaisePropertyChanged("GamesListFlyOutState");

                //Set the previous game to inactive and no new messages
                _prevGame._activeGame = false;
                _prevGame._hasNewMessage = false;
            }
        }

        private MuCommand _selectedCmd = null;
        public MuCommand SelectedCmd
        {
            get { return _selectedCmd; }
            set
            {
                //if (value == _selectedCmd)
                //    _selectedCmd = value;
                //RaisePropertyChanged("SelectedCmd");

                SubmitSelectedCmd(value);
                _selectedCmd = null;
            }
        }

        private void SubmitSelectedCmd(MuCommand _selectedCmd)
        {
            var cmd = _gameCommands[_selectedCmd.ID];
            Messenger.Default.Send<MuCommand>(cmd, "SelectedCmd");
            _commonCmdsFlyOutState = false;
            RaisePropertyChanged("CommonCmdsFlyOutState");
            //_selectedCmd = null;
            RaisePropertyChanged("SelectedCmd");
            Messenger.Default.Send(new NotificationMessage("ResetSelectedCommand"));
        }

        
        public GameViewModel CurrentGame
        {
            get
            {
                //if (_currentGame == null)
                //{
                //    _currentGame = new GameViewModel(Token);
                //}
                return _currentGame;
            }
            set
            {
                if (_currentGame == value)
                    return;
                _currentGame = value;
                RaisePropertyChanged(() => "CurrentGamePropertyName");
            }
        }

        private GameViewModel GetSelectedGameByID(int _selectedGame)
        {
            //for (int i = 0; i < GameVMList.Count; i++)
            //{
            //    if (GameVMList[i]) return GameVMList[i];
            //}
            return GameVMList[_selectedGame];
        }


        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}