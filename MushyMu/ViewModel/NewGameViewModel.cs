using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace MushyMu.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NewGameViewModel : ViewModelBase
    {
        
        /// <summary>
        /// Initializes a new instance of the NewGameViewModel class.
        /// </summary>

        public RelayCommand QuickNewGameConnect { get; private set; }
        public RelayCommand<Game> OpenGameCommand { get; private set; }
        
        public NewGameViewModel()
        {
            QuickNewGameConnect = new RelayCommand(() => ExecuteQuickNewGameConnect());
            OpenGameCommand = new RelayCommand<Game>((g) => ExecuteOpenGameCommand(g));
        }

        private void ExecuteQuickNewGameConnect()
        {
            var msg = new Game() { Name = "Test", Host = "Test", Port = 1111, ID = Guid.NewGuid()};
            Messenger.Default.Send<Game>( msg, "StartGame");
        }

        private void ExecuteOpenGameCommand(Game g)
        {
            var msg = new Game() { Name = g.Name, Host = g.Host, Port = g.Port, ID = Guid.NewGuid() };
            Messenger.Default.Send<Game>(msg, "StartGame");
        }

        // Generate List of Games from the settings XML file in Program Data.
        // May need to handle putting this in AppData or Documents instead for easier access.

        public static List<Game> ListGames()
        {
            List<Game> _games = new List<Game>();
            //DirectoryInfo _gameFile = new DirectoryInfo(@"C:\ProgramData\MushyMu\MushyMuSettings.xml");
            XmlDocument gamesXML = new XmlDocument();
            gamesXML.Load(@"C:\ProgramData\MushyMu\MushyMuSettings.xml");
            
            XmlNode root = gamesXML.SelectSingleNode("//games");
            XmlNodeList nodeList = root.SelectNodes("game");
            
            foreach (XmlNode n in nodeList)
            {
                Game g = new Game();
                g.Name = n.SelectSingleNode("name").InnerText;
                g.Host = n.SelectSingleNode("host").InnerText;
                g.Port = Convert.ToInt32(n.SelectSingleNode("port").InnerText);

                _games.Add(g);
            }
            return _games;
        }

        private List<Game> _games = ListGames();
        public List<Game> Games
        {
            get
            {
                return _games;
            }
            set
            {
                _games = value;
            }
           
        }

         

    }
}