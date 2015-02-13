using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MushyMu.ViewModel
{
    /// <summary>
    /// This class contains everything related to an individual game instance. 
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GameViewModel : ViewModelBase, IDisposable
    {
        
        #region Declarations

        public string Token;
        public string _mushName;
        public string _mushHost;
        public int _mushPort;
        public Guid _mushID;
        public string _charName;
        public string _charPW;
        public RelayCommand SubmitTextEnterKeyCommand { get; private set; }
        public RelayCommand SwitchGamesFlyOut { get; private set; }
        public RelayCommand CommonCmdsFlyOut { get; private set; }
        public RelayCommand InputHistoryFlyOut { get; private set; }
        public RelayCommand GameSettingsFlyOut { get; private set; }
        public RelayCommand WrappingOnOff { get; private set; }
        public RelayCommand CloseGame { get; private set; }
        public RelayCommand Reconnect { get; private set; }
        public RelayCommand InsertBookMark { get; private set; }
        public RelayCommand PageUpKeyCommand { get; private set; }
        public RelayCommand PageDownKeyCommand { get; private set; }
        public RelayCommand OpenPreviousCommandFlyOut { get; private set; }
        public RelayCommand OpenTextEditor { get; private set; }
        private TcpClient _client;
        private Thread _thread;
        public NetworkStream _stream;
        public Action<string> _updateMethod;
        public ConnectionInfo _connectInfo;
        private IntPtr nativeRes = Marshal.AllocHGlobal(100);
        
        //a parser/decoder for ANSI control sequences, to give text color and potentially other styling
        ANSIColorParser ansiColorParser = new ANSIColorParser();
        
        // Create FlowDocument Output
        FlowDocument output = new FlowDocument();
        
        //Wrapping variables
        public double _pageWidth;
        public double _pageWidthBuffer;

      
        //Logging variables
        TextPointer startPos;

        

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
            GameSettingsFlyOut = new RelayCommand(() => ExecuteGameSettingsFlyOut());
            WrappingOnOff = new RelayCommand(() => ExecuteWrappingOnOff());
            CloseGame = new RelayCommand(() => ExecuteCloseGame());
            Reconnect = new RelayCommand(() => ExecuteReconnect());
            InsertBookMark = new RelayCommand(() => ExecuteInsertBookMark());
            PageUpKeyCommand = new RelayCommand(() => ExecutePageUpKeyCommand());
            PageDownKeyCommand = new RelayCommand(() => ExecutePageDownKeyCommand());
            OpenPreviousCommandFlyOut = new RelayCommand(() => ExecuteInputHistoryFlyOut());
            OpenTextEditor = new RelayCommand(() => ExecuteOpenTextEditor());
            _reconnectColor = Brushes.Goldenrod;
            _disconnectColor = Brushes.Firebrick;
            RaisePropertyChanged("ReconnectColor");
            RaisePropertyChanged("DisconnectColor");


            output.IsEnabled = true;
            output.IsOptimalParagraphEnabled = true;

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

            // Define Main View Model


        }

        #endregion

        #region Command Methods

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        
        


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
                Disconnect();

            }
            if (nativeRes != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeRes);
                nativeRes = IntPtr.Zero;
            }
            
        }

        private void ExecuteInsertBookMark()
        {
            //Insert bookmark at cursor position

        }

        private void ExecuteGameSettingsFlyOut()
        {
            _gameSettingsFlyOutState = true;
            RaisePropertyChanged("GameSettingsFlyOutState");
        }

        private void ExecuteInputHistoryFlyOut()
        {
            InputHistoryFlyOutState = true;
            //SelectedInputHistoryItem = -1;
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

        private void ExecuteCloseGame()
        {
            //Disconnect from the Stream and then send a message to the main viewmodel to remove this from the list. Finally dispose of this viewmodel.
            Disconnect();
            EchoInput("> Disconnected from " + _mushName + " @ " + System.DateTime.Now);
            
        }

        private void ExecuteReconnect()
        {
            //Disconnect the game and then re-initiate the connection.
            Disconnect();
            EchoInput("> Reconnecting to " + _mushName + " @ " + System.DateTime.Now);
            Connect();
            
        }

        private void ExecutePageUpKeyCommand()
        {
            Messenger.Default.Send(new NotificationMessage("PageUpCommand"));
            
        }

        private void ExecutePageDownKeyCommand()
        {
            Messenger.Default.Send(new NotificationMessage("PageDownCommand"));
        }

        private object ExecuteSubmitTextEnterKeyCommand()
        {
            var _text = _mushTextInput;
            MushTextInput = String.Empty;
            Send(_text);
            InputHistory.Insert(0, _text);
            //SelectedInputHistoryItem = -1;
            return null;
        }

        private void ExecuteOpenTextEditor()
        {
            Messenger.Default.Send<GameViewModel>(this, "OpenTextEditor");
        }

        #endregion

        #region Connection & Communication Methods

        /// <summary>
        /// Handle game connection, communication thread and adding paragraphs to the output here
        /// </summary>
        /// <param name="StartNewGame"></param>
 
        private object StartNewGame(Game _game)
        {
            
            // Grab all parameters for this game and store them for later use.
            _mushName = _game.Name;
            RaisePropertyChanged("MushName");
            _mushHost = _game.Host;
            _mushPort = _game.Port;
            _mushID = _game.ID;
            _charName = _game.CharName;
            _charPW = _game.Password;
            _selectedFont = new FontFamily(_game.Font);
            RaisePropertyChanged("SelectedFont");
            _selectedFontSize = _game.FontSize;
            RaisePropertyChanged("SelectedFontSize");

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
                if (!String.IsNullOrEmpty(_charName) && !String.IsNullOrEmpty(_charPW))
                {
                    do
                    {
                        Send("connect " + _charName + " " + _charPW);
                    }
                    while (_client.Available < 0);
                }

                _reconnectColor = Brushes.Goldenrod;
                _disconnectColor = Brushes.Firebrick;
                RaisePropertyChanged("DisconnectColor");
                RaisePropertyChanged("ReconnectColor");
            }
            catch
            {
                EchoInput("Unable to connect to: " + _mushName + " @ " + _mushHost + ":" + _mushPort);
                _reconnectColor = Brushes.Green;
                _disconnectColor = Brushes.DarkGray;
                RaisePropertyChanged("DisconnectColor");
                RaisePropertyChanged("ReconnectColor");
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
                //Console.WriteLine("Connection to " + _mushName + " Lost: " + _mushHost + ":" + _mushPort);
                Console.WriteLine("An exception ocurred: '{0}'", ex);
            }
        }


        public void HandleDataReceived(string input)
        {
            //System.Diagnostics.Debug.WriteLine(input);
            //string result = input.Replace("\0", string.Empty)

            string page1 = "pages:";
            string page2 = "You paged";
            string page3 = "Long distance to";
            string page4 = "(To:";
            string page5 = "Long distance from";
            string page6 = "From afar,";

            //Check to see if the incoming text is a page
            if (input.Contains(page1) || input.StartsWith(page2) || input.StartsWith(page3) || input.StartsWith(page4) || input.StartsWith(page5) || input.StartsWith(page6))
            {
                input = "--> " + input;
            }
                        

            AnsiParseText(input);
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
                        //If it's a page, bold it.
                        if(r.Content.ToString().StartsWith("-->"))
                        {
                            rtf.FontStyle = FontStyles.Italic;
                        }

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
               
                    }

                    AppendMushTextStream(myParagraph);
                });
        }



        public void Disconnect()
        {
            try
            {
                _stream.Dispose();

                _stream.Close();

                _client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            _reconnectColor = Brushes.Green;
            _disconnectColor = Brushes.DarkGray;
            RaisePropertyChanged("DisconnectColor");
            RaisePropertyChanged("ReconnectColor");
            
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
            
           
    
            
            if (_loggingOnOff == true)
                {
                    startPos = output.Blocks.LastBlock.ContentEnd;
                    output.Blocks.Add(text);
                    _mushTextStream = output;
                    RaisePropertyChanged("MushTextStream");
                    LogIncomingText(_logFile, startPos);
                
                }
                else
                {
                    output.Blocks.Add(text);
                    _mushTextStream = output;
                    RaisePropertyChanged("MushTextStream");
                    
                }
            
            
            // If window is minimized, flash it.
            if(!ApplicationIsActivated())
            {
                //Send message to flash window
                Messenger.Default.Send(new NotificationMessage("FlashWindow"));
            };

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

        

        private void SubmitInputHistoryItem(int _selectedInputHistoryItem)
        {
            try
            {
                var cmd = _inputHistory[_selectedInputHistoryItem];
                InputHistoryFlyOutState = false;
                MushTextInput = cmd;
                //Messenger.Default.Send(new NotificationMessage("ResetInputHistoryIndex"));
                Messenger.Default.Send(new NotificationMessage("ResetTextBoxFocus"));
                ResetInputHistorySelection();
            }
            catch (ArgumentOutOfRangeException e)
            {
                System.Console.WriteLine(e.Message);
                //ResetInputHistorySelection();
                
            }
            
        }

        private void ResetInputHistorySelection()
        {
            SelectedInputHistoryItem = -1;
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
                Messenger.Default.Send(new NotificationMessage("ResetTextBoxFocus"));
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

        public void LogWithHistory()
        {
            string now = System.DateTime.Now.ToString("o");
            string nowDT = now.Replace(":", ".");
            string name = _mushName.Replace(":", "");
            string _logName = string.Format(name + "-" + nowDT + ".txt");
            _logFile = System.IO.Path.Combine(_logSaveLocation, _logName);
            RaisePropertyChanged("LogFile");
            LogHistory(_logFile);
            string message = "> Started logging with history @" + System.DateTime.Now + ":" + _logFile;
            EchoInput(message);
        }

        public void LogHistory(string logFile)
        {
            //Save full backlog of output to an RTF file. Still need to handle incoming next.
            using (FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                TextRange textRange = new TextRange(
                    output.ContentStart,
                    output.ContentEnd);
                    textRange.Save(fs, DataFormats.Text);
            }
        }

        public void LogNoHistory()
        {
            //No backlog, so echo to the output and initiate logging.
            string now = System.DateTime.Now.ToString("o");
            string nowDT = now.Replace(":", ".");
            string name = _mushName.Replace(":", "");
            string _logName = string.Format(name + "-" + nowDT + ".txt");
            _logFile = System.IO.Path.Combine(_logSaveLocation, _logName);
            RaisePropertyChanged("LogFile");
            string message = "> Started logging without history @" + System.DateTime.Now + ":" + _logFile;
            EchoInput(message);
        }

        public void LogIncomingText(string _logFile, TextPointer _currentPosition)
        {
            
            //using (FileStream fs = new FileStream(_logFile, FileMode.Append, FileAccess.Write))
            //{
            //    TextRange textRange = new TextRange(
            //        _currentPosition,
            //        output.ContentEnd);
               
            //    string outputTest = textRange.Text.Trim();
            //    if (!String.IsNullOrEmpty(outputTest))
            //    {
            //        textRange.Save(fs, DataFormats.Text);
            //        fs.Flush();
            //        fs.Close();
            //    }
            //}

            TextRange textRange = new TextRange(
                    _currentPosition,
                    output.ContentEnd);
            
            StringBuilder sb = new StringBuilder();

            sb.Append(textRange.Text.Trim());
            sb.AppendLine();

            using (StreamWriter outputLog = new StreamWriter(_logFile, true))
            {
                outputLog.WriteAsync(sb.ToString());
            }

        }

        #endregion

        #region Properties and Lists

        //private double _scrollViewerOffSet;
        //public double ScrollViewerOffSet
        //{
        //    get { return _scrollViewerOffSet; }
        //    set
        //    {
        //        _scrollViewerOffSet = value;
        //        RaisePropertyChanged("ScrollViewerOffSet");
        //    }
        //}

        private SolidColorBrush _reconnectColor;
        public SolidColorBrush ReconnectColor
        {
            get { return _reconnectColor; }
            set
            {
                _reconnectColor = value;
                RaisePropertyChanged("ReconnectColor");
            }
        }

        private SolidColorBrush _disconnectColor;
        public SolidColorBrush DisconnectColor
        {
            get { return _disconnectColor; }
            set
            {
                _disconnectColor = value;
                RaisePropertyChanged("DisconnectColor");
            }
        }

        private string _logFile;
        public string LogFile
        {
            get { return _logFile; }
            set
            {
                if (_logFile == value)
                    return;
                _logFile = value;
                RaisePropertyChanged("LogFile");
                
            }
        }

        private bool _loggingOnOff = false;
        public bool LoggingOnOff
        {
            get { return _loggingOnOff; }
            set
            {
                if (value == true)
                {
                    _loggingOnOff = value;
                    RaisePropertyChanged("LoggingOnOff");
                
                    //Start logging here
                    if (_loggingHistoryOnOff == true)
                    {
                        //Log with history
                        LogWithHistory();
                    }
                    else
                    {
                        //Log without history
                        LogNoHistory();
                    }
                }
                else
                {
                    //Stop logging here
                    EchoInput("> Logging stopped");
                    _loggingOnOff = value;
                    RaisePropertyChanged("LoggingOnOff");
                }
            }
        }

        private bool _loggingHistoryOnOff = true;
        public bool LoggingHistoryOnOff
        {
            get { return _loggingHistoryOnOff; }
            set
            {
                _loggingHistoryOnOff = value;
                RaisePropertyChanged("LoggingHistoryOnOff");
                if (value == true)
                {
                    //Check to see if logging has already started. If so, can't start history buffer until it's stopped.
                    if (_loggingOnOff == true)
                    {
                        //Can't log history buffer
                    }
                    else
                    {
                        //Do nothing 
                    }
                }
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
        
        private string _logSaveLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),@"MushyMu\Logs");
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
            set
            {
                if (_inputHistory != value)
                {
                    _inputHistory = value;
                    RaisePropertyChanged("InputHistory");
                }
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
                //Messenger.Default.Send(new NotificationMessage("ResetInputHistoryIndex"));
            }
        }


        private int _selectedInputHistoryItem = -1;
        public int SelectedInputHistoryItem
        {
            get { return _selectedInputHistoryItem; }
            set
            {
                if (_selectedInputHistoryItem == value)
                {
                    return;
                }
                _selectedInputHistoryItem = value;
                RaisePropertyChanged("SelectedInputHistoryItem");
                if( _selectedInputHistoryItem != -1)
                {
                    SubmitInputHistoryItem(value);
                }
                    
            }
        }

        

        public void InitConnectText()
        {
                string connectmsg = "> Connecting to ";
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
                //ScrollToEnd(output);
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