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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
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
        public RelayCommand InputHistoryFlyOut { get; private set; }
        public RelayCommand SearchText { get; private set; }
        public RelayCommand GameSettingsFlyOut { get; private set; }
        private TcpClient _client;
        private Thread _thread;
        public NetworkStream _stream;
        public Action<string> _updateMethod;
        public ConnectionInfo _connectInfo;
        
        //a parser/decoder for ANSI control sequences, to give text color and potentially other styling
        ANSIColorParser ansiColorParser = new ANSIColorParser();
        
        // Create FlowDocument Output
        FlowDocument output = new FlowDocument();

        public GameViewModel(string token)
        {
            Token = token;
            Messenger.Default.Register<Game>(this, token, (_game) => StartNewGame(_game));
            Messenger.Default.Register<MuCommand>(this, "SelectedCmd" ,(_cmd) => SubmitCommand(_cmd));
            
            SubmitTextEnterKeyCommand = new RelayCommand(() => ExecuteSubmitTextEnterKeyCommand());
            SwitchGamesFlyOut = new RelayCommand(() => ExecuteSwitchGamesFlyOut());
            CommonCmdsFlyOut = new RelayCommand(() => ExecuteCommonCmdsFlyOut());
            InputHistoryFlyOut = new RelayCommand(() => ExecuteInputHistoryFlyOut());
            SearchText = new RelayCommand(() => ExecuteSearchText());
            GameSettingsFlyOut = new RelayCommand(() => ExecuteGameSettingsFlyOut());
            output.IsEnabled = true;

            // Construct font size list
            _fontSizes.Add(10);
            _fontSizes.Add(11);
            _fontSizes.Add(12);
            _fontSizes.Add(13);
            _fontSizes.Add(14);
            _fontSizes.Add(16);
            _fontSizes.Add(18);

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

        private void ExecuteGameSettingsFlyOut()
        {
            _gameSettingsFlyOutState = true;
            RaisePropertyChanged("GameSettingsFlyOutState");
        }

        private void ExecuteInputHistoryFlyOut()
        {
            _inputHistoryFlyOutState = true;
            RaisePropertyChanged("InputHistoryFlyOutState");
        
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
                EchoInput("Unable to connect to: " + _mushName + " @ " + _mushHost + ":" + _mushPort);
            }
        }


        private void CommThread()
        {
            string line;
            StreamReader lineRead = new StreamReader(_stream);
            try
            {
                while ((line = lineRead.ReadLine()) != null)
                {
                    line = Regex.Replace(line, @"[^\u0000-\u00FD]", string.Empty);
                    HandleDataReceived(line);

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Connection to " + _mushName + " Lost: " + _mushHost + ":" + _mushPort + ex);
            }
        }


        public void HandleDataReceived(string input)
        {
            //System.Diagnostics.Debug.WriteLine(input);
            string result = input.Replace("\0", string.Empty);
            AnsiParseText(result);
        }

        public void AnsiParseText(string text)
        {

            //System.Diagnostics.Debug.WriteLine(text);
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
                        
                        //Before adding the line to the paragraph, check to see if it contains a url link. If so, split it to runs before and after the link and add the link separately.
                        //This still needs to be done.
                        myParagraph.Inlines.Add(rtf);
                        
                        //if (r.UnderLined == true)
                        //{
                        //    myParagraph.TextDecorations = TextDecorations.Underline;
                        //}
                        myParagraph.FontFamily = _selectedFont;
                        myParagraph.FontSize = _fontSize;
                        myParagraph.LineHeight = 0.005;
                        myParagraph.Margin = new Thickness(0);
                        //myParagraph.KeepWithNext = true;
                        //myParagraph.KeepTogether = true;
                    }

                    AppendMushTextStream(myParagraph);
                    //AppendMushTextStream(myParagraph);
                    //ScrollViewer.ScrollToBottom();
                });
        }



        private void Disconnect()
        {
            throw new NotImplementedException();
        }

        internal void AppendMushTextStream(Paragraph text)
        {

            // If ActiveGame = false and HasNewMessage = false, set HasNewMessage to true;
            if (_activeGame == false && _hasNewMessage == false)
            {
                _hasNewMessage = true;
                RaisePropertyChanged("HasNewMessage");
                string newMessage = "----> New Activity @ " + System.DateTime.Now;
                EchoInput(newMessage);
            }
            

            output.Blocks.Add(text);
            _mushTextStream = output;
            RaisePropertyChanged("MushTextStream");
            ScrollToBottom(text);
            
            //For diagnostic purposes, this will take the current paragraph and display it as XAML for debugging.
            //Slows down output stream quite a bit.
            //This is SPECIFICALLY to test for XAML issues, like the extra blank paragraph being added for empty
            //lines that come back through the tcp stream.
            //string textOutput = XamlWriter.Save(text);
            //System.Diagnostics.Debug.WriteLine(textOutput);
            
            
        }

        public void Send(string input)
        {

            string text;

            //if not connected, do nothing
            if (!this._client.Connected) return;

            //add carriage return and line feed
            text = input + "\r\n";

            //send to server
            _stream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
            
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
            echo.KeepWithNext = true;
            AppendMushTextStream(echo);

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

        private ObservableCollection<String> _inputHistory = new ObservableCollection<String>();
        public ObservableCollection<String> InputHistory
        {
            get
            {
                return _inputHistory;
            }
        }

        private object ExecuteSubmitTextEnterKeyCommand()
        {
            var _text = _mushTextInput;
            _mushTextInput = String.Empty;
            RaisePropertyChanged("MushTextInput");
            Send(_text);
            _inputHistory.Add(_text);
            RaisePropertyChanged("InputHistory");
            return null;
        }

        private bool _gameSettingsFlyOutState;
        public bool GameSettingsFlyOutState
        {
            get
            {
                return _gameSettingsFlyOutState;
            }
            set
            {
                _gameSettingsFlyOutState = value;
                RaisePropertyChanged("GameSettingsFlyOutState");
            }
        }

        private bool _inputHistoryFlyOutState;
        public bool InputHistoryFlyOutState
        {
            get
            {
                return _inputHistoryFlyOutState;
            }
            set
            {
                _inputHistoryFlyOutState = value;
                RaisePropertyChanged("InputHistoryFlyOutState");
            }
        }


        private int _selectedInputHistoryItem = -1;
        public int SelectedInputHistoryItem
        {
            get { return _selectedInputHistoryItem; }
            set
            {
                SubmitInputHistoryItem(value);
                _selectedInputHistoryItem = -1;
            }
        }

        private void SubmitInputHistoryItem(int _selectedInputHistoryItem)
        {
            var cmd = _inputHistory[_selectedInputHistoryItem];
            _inputHistoryFlyOutState = false;
            RaisePropertyChanged("InputHistoryFlyOutState");
            _mushTextInput = cmd;
            RaisePropertyChanged("MushTextInput");
            Messenger.Default.Send(new NotificationMessage("ResetInputHistoryIndex"));
            Messenger.Default.Send(new NotificationMessage("ResetTextBoxFocus"));
        }

        public void InitConnectText()
        {
                string connectmsg = "Connecting to ";
                string connectinfo = _mushHost + ":" + _mushPort;
                EchoInput(connectmsg + _mushName + " @ " + connectinfo +  "..." );
         
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

        private FontFamily _selectedFont = new FontFamily("Courier New");
        public FontFamily SelectedFont
        {
            get { return _selectedFont; }
            set
            {
                if (_selectedFont == value)
                    return;
                _selectedFont = value;
                RaisePropertyChanged("SelectedFont");
            }
        }


        private List<int> _fontSizes = new List<int>();
        public List<int> FontSizes
        {
            get { return _fontSizes; }
            set
            {
                _fontSizes = value;

            }
        }

        private int _fontSize = 14;
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                if (_fontSize == value)
                    return;
                _fontSize = value;
                RaisePropertyChanged("FontSize");
            }
        }

        public bool _activeGame = true;
        public bool ActiveGame
        {
            get { return _activeGame; }
            set
            {
                if (_activeGame == value)
                    return;
                _activeGame = value;
                RaisePropertyChanged("ActiveGame");
                ScrollToEnd(output);
            }
        }

        public bool _hasNewMessage = false;
        public bool HasNewMessage
        {
            get { return _hasNewMessage; }
            set
            {
                if (_hasNewMessage == value)
                    return;
                _hasNewMessage = value;
                RaisePropertyChanged("HasNewMessage");
            }
        }

        public void ScrollToBottom(Paragraph text)
        {
            text.BringIntoView();
        }

        public void ScrollToEnd(FlowDocument text)
        {
            text.BringIntoView();
        }
       
    }
}