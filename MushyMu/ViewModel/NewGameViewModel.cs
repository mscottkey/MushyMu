using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using MushyMu.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

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
        public RelayCommand SaveGameCommand { get; private set; }
        
        public NewGameViewModel()
        {
            QuickNewGameConnect = new RelayCommand(() => ExecuteQuickNewGameConnect());
            OpenGameCommand = new RelayCommand<Game>((g) => ExecuteOpenGameCommand(g));
            SaveGameCommand = new RelayCommand(() => ExecuteSaveGameCommand());
        }

        private void ExecuteSaveGameCommand()
        {
            //Check to see if Quick connect settings have been filled out
            if (_quickConnectName != null & _quickConnectHost != null & _quickConnectPort != null)
            {
                string gamesXMLpath = @"C:\ProgramData\MushyMu\MushyMuSettings.xml";

                XDocument document = XDocument.Load(gamesXMLpath);
                
                document.Element("root").Element("games").Add(new XElement("game",
                new XElement("name", _quickConnectName),
                new XElement("host", _quickConnectHost),
                new XElement("port", _quickConnectPort)));

                document.Save(gamesXMLpath);

                _games = ListGames();
                RaisePropertyChanged("Games");

                _quickConnectName = null;
                _quickConnectHost = null;
                _quickConnectPort = null;
                RaisePropertyChanged("QuickConnectName");
                RaisePropertyChanged("QuickConnectHost");
                RaisePropertyChanged("QuickConnectPort");

                Messenger.Default.Send(new NotificationMessage("DialogSaveComplete"));


            }
            else
            {
                Messenger.Default.Send(new NotificationMessage("DialogIncompleteConnectionInformation"));
                
            }

        }

        private void ExecuteQuickNewGameConnect()
        {
            if (_quickConnectName == null && _quickConnectHost == null && _quickConnectPort == null)
            {
                Messenger.Default.Send(new NotificationMessage("DialogIncompleteCnnectionInformation"));
            }
            else
            {
                var msg = new Game() { Name = _quickConnectName, Host = _quickConnectHost, Port = Convert.ToInt32(_quickConnectPort), ID = Guid.NewGuid() };
                Messenger.Default.Send<Game>(msg, "StartGame");
            }
        }

        private void ExecuteOpenGameCommand(Game g)
        {
            var msg = new Game() { Name = g.Name, Host = g.Host, Port = g.Port, ID = Guid.NewGuid() };
            Messenger.Default.Send<Game>(msg, "StartGame");
        }
        
        // QuickConnect and Save field properties
        private string _quickConnectName;

        public string QuickConnectName
        {
            get { return _quickConnectName; }
            set 
            { 
                _quickConnectName = value;
                RaisePropertyChanged("QuickConnectName");
            }
        }

        private string _quickConnectHost;

        public string QuickConnectHost
        {
            get { return _quickConnectHost; }
            set 
            { 
                _quickConnectHost = value;
                RaisePropertyChanged("QuickConnectHost");
            }
        }

        private string _quickConnectPort;

        public string QuickConnectPort
        {
            get { return _quickConnectPort; }
            set 
            { 
                _quickConnectPort = value;
                RaisePropertyChanged("QuickConnectPort");
            }
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
                RaisePropertyChanged("Games");
            }
           
        }

         

    }
}