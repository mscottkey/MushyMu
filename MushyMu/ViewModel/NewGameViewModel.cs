﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using MushyMu.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
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
            // Should also check to see if data is valid

            //Check to see if Quick connect settings have been filled out
            if (_quickConnectName != null & _quickConnectHost != null & _quickConnectPort != null)
            {
                 TcpClient _client = new TcpClient();
                 try
                 {
                     _client.Connect(_quickConnectHost, Convert.ToInt32(_quickConnectPort));
                     _client.Close();
                     string gamesXMLpath = @"C:\ProgramData\MushyMu\MushyMuSettings.xml";

                     XDocument document = XDocument.Load(gamesXMLpath);

                     document.Element("root").Element("games").Add(new XElement("game",
                     new XElement("name", _quickConnectName),
                     new XElement("host", _quickConnectHost),
                     new XElement("port", _quickConnectPort),
                     new XElement("font", "Courier New"),
                     new XElement("fontsize", 12),
                     new XElement("charname", _quickConnectCharName),
                     new XElement("password", _quickConnectPassword)));
                    
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
                catch
                 {
                    //Invalid game info
                     Messenger.Default.Send(new NotificationMessage("InvalidConnectInfo"));
                 }


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
                Messenger.Default.Send(new NotificationMessage("InvalidConnectInfo"));
            }
            else
            {
                var msg = new Game() { Name = _quickConnectName, Host = _quickConnectHost, Port = Convert.ToInt32(_quickConnectPort), ID = Guid.NewGuid(), Font = "Courier New", FontSize = 12, 
                    CharName = _quickConnectCharName, Password = _quickConnectPassword };
                Messenger.Default.Send<Game>(msg, "StartGame");
            }
        }

        private void ExecuteOpenGameCommand(Game g)
        {
            var msg = new Game() { Name = g.Name, Host = g.Host, Port = g.Port, ID = Guid.NewGuid(), Font = g.Font, FontSize=g.FontSize,
                CharName = g.CharName, Password = g.Password };
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

        private string _quickConnectCharName;

        public string QuickConnectCharName
        {
            get { return _quickConnectCharName; }
            set
            {
                _quickConnectCharName = value;
                RaisePropertyChanged("QuickConnectCharName");
            }
        }
       
        private string _quickConnectPassword;

        public string QuickConnectPassword
        {
            get { return _quickConnectPassword; }
            set
            {
                _quickConnectPassword = value;
                RaisePropertyChanged("QuickConnectPassword");
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
                g.Font = n.SelectSingleNode("font").InnerText;
                g.FontSize = Convert.ToInt32(n.SelectSingleNode("fontsize").InnerText);
                g.CharName = n.SelectSingleNode("charname").InnerText;
                g.Password = n.SelectSingleNode("password").InnerText;

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