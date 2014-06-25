using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MushyMu.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GameViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the GameViewModel class. This holds anything related to a specific game instance. 
        /// </summary>

        public string Token;
        public string _mushName;
        public string _mushHost;
        public int _mushPort;
        public Guid _mushID;
        public RelayCommand SubmitTextEnterKeyCommand { get; private set; }
        private TcpClient _client;
        private Thread _thread;
        public NetworkStream _stream;
        public Action<string> _updateMethod;
        public ConnectionInfo _connectInfo;
        //a parser/decoder for ANSI control sequences, to give text color and potentially other styling
        ANSIColorParser ansiColorParser = new ANSIColorParser();

        public GameViewModel(string token)
        {
            Token = token;
            Messenger.Default.Register<Game>(this, token, (_game) => StartNewGame(_game));
            //MessengerInstance.Register<Game>(this, )
            SubmitTextEnterKeyCommand = new RelayCommand(() => ExecuteSubmitTextEnterKeyCommand());
        }

        private object StartNewGame(Game _game)
        {
            
            // Grab all parameters for this game and store them for later use.
            _mushName = _game.Name;
            RaisePropertyChanged("MushName");
            _mushHost = _game.Host;
            _mushPort = _game.Port;
            _mushID = _game.ID;

            // Send connection text.
            InitConnectText();

            // Start Connection
            Connect();
            return null;
        }

        public void Connect()
        {

            _updateMethod = HandleDataReceived;
            _client = new TcpClient();
            try
            {
                _client.Connect(_mushHost, _mushPort);
                _stream = _client.GetStream();
                _thread = new Thread(CommThread) { IsBackground = true };
                _thread.Start();
            }
            catch
            {
                _updateMethod("Unable to connect to:" + _mushName + " @ " + _mushHost + ":" + _mushPort);
            }
        }

        private void CommThread()
        {
            string line;
            StreamReader lineRead = new StreamReader(_stream);
            while ((line = lineRead.ReadLine()) != null)
            {
                HandleDataReceived(line);
            }
        }

        public void HandleDataReceived(string lineOfData)
        {
            AppendMushTextStream(lineOfData);
            System.Diagnostics.Debug.WriteLine(lineOfData);
        }

        public string MushName
        {
            get { return _mushName; }
            set
            {
                if (value == _mushName)
                    return;
                _mushName = value;
                RaisePropertyChanged("MushName");
            }
        }
        

        private object ExecuteSubmitTextEnterKeyCommand()
        {
            var _text = _mushTextInput;
            _mushTextInput = String.Empty;
            RaisePropertyChanged("MushTextInput");
            AppendMushTextStream(_text);
            Send(_text);
            return null;
        }

        public void InitConnectText()
        {
            string connectmsg = "Connecting to ";
            string connectinfo = _mushHost + ":" + _mushPort;
            AppendMushTextStream(connectmsg + _mushName + " @ " + connectinfo +  "..." );
        }

        private string _mushTextStream = String.Empty;
        public string MushTextStream
        {
            get { return _mushTextStream; }
        }

        private string _mushTextInput = String.Empty;
        public string MushTextInput
        {
            get { return _mushTextInput; }
            set { _mushTextInput = value; RaisePropertyChanged("MushTextInput"); }
        }

        internal void AnsiParseText(string text)
        {
            List<AnsiTextRun> runs = this.ansiColorParser.Parse(text);

        }

        internal void AppendMushTextStream(string text)
        {
            
            _mushTextStream += text + Environment.NewLine;
            RaisePropertyChanged("MushTextStream");
            
        }

        public void Send(string input)
        {
            string data;
            data = input + "\n";
            _stream.Write(Encoding.ASCII.GetBytes(data), 0, data.Length);
            System.Diagnostics.Debug.WriteLine(data);
        }


       
    }
}