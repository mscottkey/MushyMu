using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using MushyMu.Views;
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
        public RelayCommand SwitchGamesFlyOut { get; private set; }
        public RelayCommand CommonCmdsFlyOut { get; private set; }
        public RelayCommand SearchText { get; private set; }
        private TcpClient _client;
        private Thread _thread;
        public NetworkStream _stream;
        public Action<string> _updateMethod;
        public ConnectionInfo _connectInfo;
        public TelnetParser _telnetParser;
        public int TimeOutMs = 100;
        private byte[] buffer = new byte[2500];
        
        //a parser/decoder for ANSI control sequences, to give text color and potentially other styling
        ANSIColorParser ansiColorParser = new ANSIColorParser();
        
        // Create FlowDocument Output
        FlowDocument output = new FlowDocument();

        //incoming message callback and handler definition
        //public event serverMessageEventHandler serverMessage;
        //public delegate void serverMessageEventHandler(List<AnsiTextRun> runs);

        public GameViewModel(string token)
        {
            Token = token;
            Messenger.Default.Register<Game>(this, token, (_game) => StartNewGame(_game));
            Messenger.Default.Register<MuCommand>(this, "SelectedCmd" ,(_cmd) => SubmitCommand(_cmd));
            
            SubmitTextEnterKeyCommand = new RelayCommand(() => ExecuteSubmitTextEnterKeyCommand());
            SwitchGamesFlyOut = new RelayCommand(() => ExecuteSwitchGamesFlyOut());
            CommonCmdsFlyOut = new RelayCommand(() => ExecuteCommonCmdsFlyOut());
            SearchText = new RelayCommand(() => ExecuteSearchText());
            output.IsEnabled = true;
            

        }

        private void ExecuteSearchText()
        {
            if (_searchValue != null)
            {
                //Do some stuff here
            }
            else
            {
                //Do nothing because there is nothing to search for
            }
        }

        private void SubmitCommand(MuCommand _cmd)
        {
            if (_cmd.SubmitOnSelect == true)
            {
                Send(_cmd.Name);
            }
            else
            {
                _mushTextInput = _cmd.Name;
                RaisePropertyChanged("MushTextInput");
            }

            Messenger.Default.Send(new NotificationMessage("ResetTextBoxFocus"));
        }

        private void ExecuteCommonCmdsFlyOut()
        {
            Messenger.Default.Send(new NotificationMessage("CommonCmdsFlyOut"));
        }

        private void ExecuteSwitchGamesFlyOut()
        {
            Messenger.Default.Send(new NotificationMessage("OpenGameListFlyOut"));
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

            //_updateMethod = HandleDataReceived;
            _client = new TcpClient();
            try
            {
                _client.Connect(_mushHost, _mushPort);

                ////initialize the telnet parser
                //this._telnetParser = new TelnetParser(this._client);

                ////start listening for new text
                //_client.Client.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(HandleDataReceived), null);

                ////send a WILL NAWS (negotiate about window size)
                //this._telnetParser.sendTelnetBytes((byte)Telnet.WILL, (byte)Telnet.NAWS);

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

        public void HandleDataReceived(string data)
        {
            System.Diagnostics.Debug.WriteLine(data);
            AnsiParseText(data);

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

        private string _searchValue;
        public string SearchValue
        {
            get { return _searchValue; }
            set
            {
                if (value == _searchValue)
                    return;
                _searchValue = value;
                RaisePropertyChanged("SearchValue");
            }
        }

        private object ExecuteSubmitTextEnterKeyCommand()
        {
            var _text = _mushTextInput;
            _mushTextInput = String.Empty;
            RaisePropertyChanged("MushTextInput");
            Send(_text);
            return null;
        }

        public void InitConnectText()
        {
            string connectmsg = "Connecting to ";
            string connectinfo = _mushHost + ":" + _mushPort;
            AnsiParseText(connectmsg + _mushName + " @ " + connectinfo +  "..." );
        }

        private FlowDocument _mushTextStream = null;
        public FlowDocument MushTextStream
        {
            get { return _mushTextStream; }
        }

        private string _mushTextInput = String.Empty;
        public string MushTextInput
        {
            get { return _mushTextInput; }
            set { _mushTextInput = value; RaisePropertyChanged("MushTextInput"); }
        }

        public void AnsiParseText(string text)
        {
            
            
            Application.Current.Dispatcher.BeginInvoke(
                (Action)delegate()
                {
                    
                    Paragraph myParagraph = new Paragraph();
                    
                    //pass the run to the AnsiParser to parse any ANSI control sequences (colors!)
                    List<AnsiTextRun> runs = this.ansiColorParser.Parse(text);
                    //add 'runs' to the output
                    foreach (var r in runs)
                    {
                        var rtf = new Run(r.Content);
                        rtf.Foreground = r.ForegroundColor;
                        rtf.Background = r.BackgroundColor;
                        //rtf.FontFamily = new FontFamily("Courier New");
                        //rtf.FontSize = 14.0;
                        myParagraph.Inlines.Add(rtf);
                        myParagraph.FontFamily = new FontFamily("Courier New");
                        myParagraph.FontSize = 14.0;
                        myParagraph.LineHeight = 1;
                    }
                    
                    AppendMushTextStream(myParagraph);
                    //ScrollViewer.ScrollToBottom();
                });
        }

        private void Disconnect()
        {
            throw new NotImplementedException();
        }

        internal void AppendMushTextStream(Paragraph text)
        {

            output.Blocks.Add(text);
            _mushTextStream = output;
            RaisePropertyChanged("MushTextStream");
            ScrollToBottom(text);
        }

        public void Send(string input)
        {

            string text;

            //if not connected, do nothing
            if (!this._client.Connected) return;

            //add carriage return and line feed
            text = input + "\r\n";

            ////convert from Unicode to ASCII
            //Encoder encoder = System.Text.Encoding.ASCII.GetEncoder();
            //char[] charArray = text.ToCharArray();
            //int count = encoder.GetByteCount(charArray, 0, charArray.Length, true);
            //byte[] outputBuffer = new byte[count];
            //encoder.GetBytes(charArray, 0, charArray.Length, outputBuffer, 0, true);

            //send to server
            _stream.Write(Encoding.ASCII.GetBytes(text), 0, text.Length);

            //Debug the ling and echo to client window
            System.Diagnostics.Debug.WriteLine(text);
            EchoInput(text);
        }

        public void EchoInput(string input)
        {
            Paragraph echo = new Paragraph();
            Run echoText = new Run(input);
            echoText.Foreground = Brushes.Yellow;
            echoText.Background = Brushes.Black;
            echo.Inlines.Add(echoText);
            echo.FontFamily = new FontFamily("Courier New");
            echo.FontSize = 14.0;
            echo.LineHeight = 1;
            AppendMushTextStream(echo);

        }

        public void ScrollToBottom(Paragraph text)
        {
            text.BringIntoView();
        }

       
    }
}