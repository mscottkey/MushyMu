using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using MushyMu.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    /// This class contains everything related to an individual game instance. 
    /// <para>
 /   // See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GameViewModel : ViewModelBase
    {
        
        #region Declarations

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
        public RelayCommand WrappingOnOff { get; private set; }
        private TcpClient _client;
        private Thread _thread;
        public NetworkStream _stream;
        public Action<string> _updateMethod;
        public ConnectionInfo _connectInfo;
        
        
        //a parser/decoder for ANSI control sequences, to give text color and potentially other styling
        ANSIColorParser ansiColorParser = new ANSIColorParser();
        
        // Create FlowDocument Output
        FlowDocument output = new FlowDocument();
        
        //Wrapping variables
        public double _pageWidth;
        public double _pageWidthBuffer;


        #endregion

        #region Constructor

        /// <summary>
        /// Construct Game class.
        /// </summary>
        /// <para>To Do: Load User Settings from XML file</param>


        public GameViewModel(string token)
        {
            Token = token;
            Messenger.Default.Register<Game>(this, token, (_game) => StartNewGame(_game));
            //Messenger.Default.Register<MuCommand>(this, "SelectedCmd" ,(_cmd) => SubmitCommand(_cmd));
            
            SubmitTextEnterKeyCommand = new RelayCommand(() => ExecuteSubmitTextEnterKeyCommand());
            SwitchGamesFlyOut = new RelayCommand(() => ExecuteSwitchGamesFlyOut());
            CommonCmdsFlyOut = new RelayCommand(() => ExecuteCommonCmdsFlyOut());
            InputHistoryFlyOut = new RelayCommand(() => ExecuteInputHistoryFlyOut());
            SearchText = new RelayCommand(() => ExecuteSearchText());
            GameSettingsFlyOut = new RelayCommand(() => ExecuteGameSettingsFlyOut());
            WrappingOnOff = new RelayCommand(() => ExecuteWrappingOnOff());
            output.IsEnabled = true;

            
            //Wrap Text if they have wrapping turned on

            if (_wrapping == true)
            {
                ExecuteWrappingOnOff();
                ScrollToEnd(output);
            } 
            

            // Construct font size list
            _fontSizes.Add(10);
            _fontSizes.Add(11);
            _fontSizes.Add(12);
            _fontSizes.Add(13);
            _fontSizes.Add(14);
            _fontSizes.Add(16);
            _fontSizes.Add(18);

        }

        #endregion

        #region Execute Commands

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

        public void ExecuteCommonCmdsFlyOut()
        {
            _commonCmdsFlyOutState = true;
            RaisePropertyChanged("CommonCmdsFlyOutState");
        }
       

        private void ExecuteSwitchGamesFlyOut()
        {
            Messenger.Default.Send(new NotificationMessage("OpenGameListFlyOut"));
        }

        private void ExecuteWrappingOnOff()
        {
            //Determine standard monospace character width in case they want wrapping turned on.
            FormattedText formattedText = new FormattedText("_", System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(_selectedFont.ToString()), (double)new FontSizeConverter().ConvertFrom(_selectedFontSize + "pt"), Brushes.Black);
            
            if (_wrapping == true)
            {
                Size _pageSize = new Size(formattedText.Width, formattedText.Height);
                _pageWidth = _pageSize.Width * _wrappingLength;
                _pageWidthBuffer = _pageSize.Width * 4;

               
                output.PageWidth = _pageWidth + _pageWidthBuffer;
                ScrollToEnd(output);
            }
            else
            {
                output.PageWidth = Double.NaN;
                ScrollToEnd(output);

            }

        }

        #endregion

        #region Connection & Communication Methods

        /// <summary>
        /// Handle game connection, communication thread and adding paragraphs to the output here
        /// </summary>
        /// <param name="strFilePath"></param>
 
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
                    line = Regex.Replace(line, @"[^\u0000-\u00FD]", String.Empty);
                    HandleDataReceived(line);
                    System.Diagnostics.Debug.WriteLine(line);
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
            //string result = input.Replace("\0", string.Empty);
            AnsiParseText(input);
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
                        myParagraph.FontSize = (double)new FontSizeConverter().ConvertFrom(_selectedFontSize + "pt");
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
            _stream.Close();
            _client.Close();
        }

        internal void AppendMushTextStream(Paragraph text)
        {

            // If ActiveGame = false and HasNewMessage = false, set HasNewMessage to true;
            if (_activeGame == false && _hasNewMessage == false)
            {
                _hasNewMessage = true;
                RaisePropertyChanged("HasNewMessage");
                string newMessage = "> New Activity @ " + System.DateTime.Now;
                EchoInput(newMessage);
            }
            
                output.Blocks.Add(text);
                _mushTextStream = output;
                RaisePropertyChanged("MushTextStream");
                ScrollToBottom(text);
                //string textOutput = XamlWriter.Save(text);
                //System.Diagnostics.Debug.WriteLine(textOutput); 
            
        }

        public void Send(string input)
        {

            string text;

            //if not connected, connect to the game. May want to provide pop up instead to connect or close.
            if (!this._client.Connected)
            {
                InitConnectText();
                Connect();
            }

            //add carriage return and line feed
            text = input + "\r\n";

            //send to server
            try
            { 
                _stream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
            }
            catch (Exception ex)
            {
                EchoInput("> Connection to host lost. Trying to reconnect.");
                System.Diagnostics.Debug.WriteLine(ex);
                InitConnectText();
                Connect();
            }
            //Debug the ling and echo to client window
            System.Diagnostics.Debug.WriteLine(text);
            EchoInput("> " + input);
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

        private void SubmitSelectedCmd(MuCommand _selectedCmd)
        {
            var cmd = _gameCommands[_selectedCmd.ID];
            if (cmd.SubmitOnSelect == true)
            {
                Send(cmd.Name);
            }
            else
            {
                _mushTextInput = cmd.Name;
                RaisePropertyChanged("MushTextInput");
            }
            _commonCmdsFlyOutState = false;
            RaisePropertyChanged("CommonCmdsFlyOutState");
            //_selectedCmd = null;
            RaisePropertyChanged("SelectedCmd");
            Messenger.Default.Send(new NotificationMessage("ResetSelectedCommand"));
        }

        public void EchoInput(string input)
        {
            Paragraph echo = new Paragraph();
            Run echoText = new Run(input);
            echoText.Foreground = Brushes.Orange;
            echoText.Background = Brushes.Black;
            echo.Inlines.Add(echoText);
            echo.FontFamily = new FontFamily("Courier New");
            echo.FontSize = (double)new FontSizeConverter().ConvertFrom("12pt");
            echo.LineHeight = 0.005;
            echo.KeepWithNext = true;
            echo.KeepTogether = true;
            AppendMushTextStream(echo);
        }

        public void ScrollToBottom(Paragraph text)
        {
            text.BringIntoView();
        }

        public void ScrollToEnd(FlowDocument text)
        {
            text.BringIntoView();
        }

        #endregion

        #region Logging




        #endregion

        #region Properties and Lists

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

        private string _logSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string LogSaveLocation
        {
            get
            {
                return _logSaveLocation;
            }
            set
            {
                _logSaveLocation = value;
                RaisePropertyChanged("LogSaveLocation");
            }
        }

        private bool _wrapping = true;
        public bool Wrapping
        {
            get
            {
                return _wrapping;
            }
            set
            {
                _wrapping = value;
                RaisePropertyChanged("Wrapping");
                ExecuteWrappingOnOff();
            }
        }

        private int _wrappingLength = 80;
        public int WrappingLength
        {
            get
            {
                return _wrappingLength;
            }
            set
            {
                _wrappingLength = value;
                RaisePropertyChanged("WrappingLength");
                ExecuteWrappingOnOff();
            }
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
                ExecuteWrappingOnOff();
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

        private int _selectedFontSize = 12;
        public int SelectedFontSize
        {
            get { return _selectedFontSize; }
            set
            {
                if (_selectedFontSize == value)
                    return;
                _selectedFontSize = value;
                RaisePropertyChanged("SelectedFontSize");
                ExecuteWrappingOnOff();
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

        #endregion

    }
}