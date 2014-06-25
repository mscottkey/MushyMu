using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using MushyMu.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushyMu.ViewModel
{
    public class GameContainerViewModel : ViewModelBase
    {
            public GameViewModel _currentGame;
            
            /// <summary>
            /// Gets the CurrentViewModel
            /// Changes to that property's value raise the PropertyChanged event. 
            /// </summary>
            
            public GameViewModel CurrentGame
            {
                get
                {
                    if (_currentGame == null)
                    {
                        _currentGame = new GameViewModel(Token);
                    }
                    return _currentGame;
                }
                set
                {
                    if (_currentGame == value)
                        return;
                    _currentGame = value;
                    RaisePropertyChanged("CurrentGame");
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
            
            string[] commands = {"foo", "bar", "baz", "gee"};
            private ObservableCollection<string> _gameCommands = null; 
            public ObservableCollection<string> GameCommands
            {
                get
                {
                    if (_gameCommands == null)
                    {
                        _gameCommands = new ObservableCollection<string>(commands);
                    }
                    return _gameCommands;
                }
            }

            /// <summary>
            /// Initializes a new instance of the GameContainerViewModel class.
            /// </summary>
            public GameContainerViewModel()
            {
                Messenger.Default.Register<Game>(this, "StartGame", (_game) => ReceiveNewGame(_game));
                //RaisePropertyChanged("GameList");
                //RaisePropertyChanged("GameVMList");

            }

            public string Token;

            private object ReceiveNewGame(Game _game)
            {
                
                //Send out game info, then generate the View Model
                Token = Guid.NewGuid().ToString();
                _currentGame = new GameViewModel(Token);
                Messenger.Default.Send<Game>(_game, Token);
                RaisePropertyChanged("CurrentGame");
                
                // Add this game to the List
                _gameList.Add(_game);
                RaisePropertyChanged("GameList");
                _gameVMList.Add(_currentGame);
                RaisePropertyChanged("GameVMList");
                _selectedGame = GameVMList.Count - 1;
                RaisePropertyChanged("SelectedGame");
                
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

                //Selection changed, switch view to that VM
                _currentGame = GetSelectedGameByID(_selectedGame);
                RaisePropertyChanged("CurrentGame");
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
