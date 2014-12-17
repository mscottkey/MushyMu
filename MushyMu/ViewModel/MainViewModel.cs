using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro;
using MushyMu.Model;
using MushyMu.Views;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Xml;
using System.Xml.Linq;


namespace MushyMu.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private bool _settingsFlyoutState;
        private ViewModelBase _currentViewModel;
        private ViewModelBase _gameContainer;
        readonly static NewGameViewModel _newGameViewModel = new NewGameViewModel();
        public RelayCommand OpenFlyoutPanelCommand { get; private set; }
        public RelayCommand<GameViewModel> CloseGameFromList { get; private set; }
        private IntPtr nativeRes = Marshal.AllocHGlobal(100);
       
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

        private ObservableCollection<Accent> _accentColors = new ThemeColorList();
        public ObservableCollection<Accent> AccentColors
        {
            get
            {
                if (_accentColors == null)
                {
                    _accentColors = new ThemeColorList();
                }
                return _accentColors;
            }
        }

        private ObservableCollection<string> _themeColors = new ObservableCollection<string>();
        public ObservableCollection<string> ThemeColors
        {
            get
            {
                if (_themeColors == null)
                {
                    _themeColors = new ObservableCollection<string>();
                }
                return _themeColors;
            }
        }

        
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _currentViewModel = MainViewModel._newGameViewModel;
            Messenger.Default.Register<int>(this, ChangeTab);
            Messenger.Default.Register<Game>(this, "StartGame", (_game) => ReceiveNewGame(_game));

            _themeColors.Add("Dark");
            _themeColors.Add("Light");

            _isGamesEnabled = false;
            
            OpenFlyoutPanelCommand = new RelayCommand(() => ExecuteOpenFlyoutPanel());
            CloseGameFromList = new RelayCommand<GameViewModel>(ExecuteCloseGameFromList);

            GetUserSettings();
        }

        private void GetUserSettings()
        {
            //Define app level XML variables
            XmlDocument settingsXML = new XmlDocument();
            settingsXML.Load(@"C:\ProgramData\MushyMu\MushyMuSettings.xml");

            XmlNode settings = settingsXML.SelectSingleNode("//settings");
            
            //Get Default Accent Color
            XmlNode colorNode = settings.SelectSingleNode("color");
            if (!String.IsNullOrEmpty(colorNode.InnerText))
            { 
                _accentChoice = colorNode.InnerText;
                RaisePropertyChanged("AccentColor");
            }
            else
            {
                _accentChoice = "Steel";
            }
        }

        //public void GameListFlyOut(bool value)
        //{
        //    _gamesListFlyOutState = true;
        //    RaisePropertyChanged("GamesListFlyOutState");
        //}

        
        public void ExecuteOpenFlyoutPanel()
        {
            _settingsFlyoutState = true;
            RaisePropertyChanged("SettingsFlyoutState");
        }

        public void ExecuteCloseGameFromList(GameViewModel game)
        {
            //Get VM from the list
            game.Disconnect();
            if (_gameVMList.Count - 1 == 0)
            {
                _isGamesEnabled = false;
                RaisePropertyChanged("IsGamesEnabled");
                _selectedTabIndex = 0;
                RaisePropertyChanged("SelectedTabIndex");
                _gameVMList.Remove(game);
                game.Cleanup();
            }
            else
            { 
                _gameVMList.Remove(game);
                RaisePropertyChanged("GameVMList");
                game.Cleanup();
                game.Dispose();
            }
        }

        //private string _themeChoice = new Theme("BaseDark");
        //public string ThemeChoice
        //{
        //    get { return _themeChoice; }
        //    set
        //    {
        //        _themeChoice = value;
        //        RaisePropertyChanged("ThemeChoice");
        //        var accent = ThemeManager.Accents.First(x => x.Name == _accentChoice);
        //        var theme = ThemeManager.AppThemes.First(x => x.Name == _themeChoice);
        //        ThemeManager.ChangeAppStyle(App.Current, accent, theme);
        //    }
        //}

        private string _accentChoice;
        public string AccentChoice
        {
            get { return _accentChoice; }
            set
            {
                _accentChoice = value;
                RaisePropertyChanged("AccentChoice");
                var accent = ThemeManager.Accents.First(x => x.Name == _accentChoice);
                var theme = ThemeManager.AppThemes.First(x => x.Name == "BaseDark");
                ThemeManager.ChangeAppStyle(App.Current, accent, theme);

                //Set theme in settings XML
                string gamesXMLpath = @"C:\ProgramData\MushyMu\MushyMuSettings.xml";

                XDocument document = XDocument.Load(gamesXMLpath);

                var element = document
                    .Element("root")
                    .Elements("settings")
                    .Elements("color")
                    .Single();
                element.Value = _accentChoice;
                document.Save(gamesXMLpath);
            }
        }


        private string _title = "MushyMu";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
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


        private bool _touchOnOff = true;
        public bool TouchOnOff
        {
            get
            {
                return _touchOnOff;
            }
            set
            {
                _touchOnOff = value;
                RaisePropertyChanged("TouchOnOff");
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
            

            //Generate the View-VM pair, then send the message out to create the game model.
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

            _title = "MushyMu - " + _currentGame._mushName;
            RaisePropertyChanged("Title");

            return null;
        }

        private int _selectedGame;
        public int SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                if (value == _selectedGame || value == -1)
                    return;
                _selectedGame = value;
                RaisePropertyChanged("SelectedGame");

                //Before selection changes, register previous game
                _prevGame = _currentGame;
               

                //Selection changed, switch view to that VM
                _currentGame = GetSelectedGameByID(_selectedGame);
                RaisePropertyChanged("CurrentGame");
                Messenger.Default.Send(new NotificationMessage("ResetTextBoxFocus"));

                //Set current game as ActiveGame
                _currentGame.ActiveGame = true;
                _currentGame.HasNewMessage = false;


                //Set the previous game to inactive and no new messages
                _prevGame._activeGame = false;
                _prevGame._hasNewMessage = false;

                //Set the title
                _title = "MushyMu - " + _currentGame._mushName;
                RaisePropertyChanged("Title");
            }
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
            try
            {
                return GameVMList[_selectedGame];

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Cleanup();
                
            }
            if (nativeRes != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeRes);
                nativeRes = IntPtr.Zero;
            }

        }

        public override void Cleanup()
        {
            // Clean up if needed

            base.Cleanup();
        }
    }
}