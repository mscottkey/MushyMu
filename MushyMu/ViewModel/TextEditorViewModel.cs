using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MushyMu.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace MushyMu.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class TextEditorViewModel : ViewModelBase
    {

        #region Declarations
        /// <summary>
        /// Flyout Menu for standard Text Editor Options.
        /// </summary>
        public RelayCommand MenuFlyOut { get; private set; }
        /// <summary>
        /// Flyout Menu for the AnsiColor Builder.
        /// </summary>
        public RelayCommand ColorPickerFlyOut { get; private set; }
        /// <summary>
        /// Setup Ansi Block and show resulting ansi code.
        /// </summary>
        public RelayCommand PreviewAnsi { get; private set; }
        /// <summary>
        /// Ansi Color block variable.
        /// </summary>
        public string ansiCodes;

        #endregion
        /// <summary>
        /// Initializes a new instance of the TextEditorViewModel class.
        /// </summary>
        public TextEditorViewModel(GameViewModel vm)
        {
            MenuFlyOut = new RelayCommand(() => ExecuteMenuFlyOut());
            ColorPickerFlyOut = new RelayCommand(() => ExecuteColorPickerFlyOut());
            PreviewAnsi = new RelayCommand(() => ExecutePreviewAnsi());
        }

       

        #region Commands

        private void ExecuteColorPickerFlyOut()
        {
            _colorPickerFlyOutState = true;
            RaisePropertyChanged("ColorPickerFlyOutState");
        }

        private void ExecuteMenuFlyOut()
        {
            _menuFlyOutState = true;
            RaisePropertyChanged("MenuFlyOutState");
        }

        private void ExecutePreviewAnsi()
        {
            _ansiCodes = ParseAnsiCodes(_selectedFG, _selectedBG, _hFG, _hBG);
            _ansiBlock = _ansiBlock1 + _ansiCodes + _ansiBlock3;
            RaisePropertyChanged("AnsiBlock");
        }

        #endregion

        #region Methods

        private string ParseAnsiCodes(AnsiColor _FG, AnsiColor _BG, bool _HFG, bool _HBG)
        {
            
            string fgCode = "w";
            string bgCode = "x";

            if (_FG.ColorName == "Black")
            {
                fgCode = "x";
            }
            else
            {
                fgCode = _FG.ColorName.Substring(0,1).ToLower();
            }

            if (_BG.ColorName == "Black")
            {
                bgCode = "X";
            }
            else
            {
                bgCode = _BG.ColorName.Substring(0,1).ToUpper();
            }


            if (_HFG == true && _HBG == true && !String.IsNullOrEmpty(fgCode) && !String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = "H" + bgCode + "h" + fgCode; // Both FG and BG are Highlighted
            }
            else if (_HFG == true && _HBG == false && !String.IsNullOrEmpty(fgCode) && !String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = bgCode + "h" + fgCode; // There is a BG Code, but FG is highlighted
            }
                else if (_HFG == true && _HBG == false && !String.IsNullOrEmpty(fgCode) && String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = "h" + fgCode; // There no BG Code and FG is highlighted
            }
            else if (_HFG == false && _HBG == true && !String.IsNullOrEmpty(fgCode) && !String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = "H" + bgCode + fgCode; //There is a FG code and BG is highlighted
            }
            else if (_HFG == false && _HBG == true && String.IsNullOrEmpty(fgCode) && !String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = "H" + bgCode; //There is no FG code and BG is highlighted
            }
            else if (_HFG == false && _HBG == false && !String.IsNullOrEmpty(fgCode) && !String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = bgCode + fgCode; // Nothing is highlighted, but there are BG and GF codes
            }
            else if (_HFG == false && _HBG == false && !String.IsNullOrEmpty(fgCode) && String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = fgCode; // Only a FG Code, no highlighting
            }
            else if (_HFG == false && _HBG == false && String.IsNullOrEmpty(fgCode) && !String.IsNullOrEmpty(bgCode))
            {
                ansiCodes = bgCode; // Only a BG Code, no highlighting
            }
            else
            {
                ansiCodes = "w"; //Something isn't right, reset it to white.
            }

            return ansiCodes;
            
        }

        #endregion

        #region Properties

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        private string _ansiBlock1 = "[ansi(";
        private string _ansiCodes = "w";
        private string _ansiBlock3 = ",<Your Text Here>)]";
        private string _ansiBlock = "[ansi(w, <Your Text Here>)]";
        public string AnsiBlock
        {
            get
            {
                return _ansiBlock;
            }
            set
            {
                _ansiBlock = value;
                RaisePropertyChanged("AnsiBlock");
            }
        }

        private ObservableCollection<AnsiColor> _ansiList = new StandardAnsiList();
        private ObservableCollection<AnsiColor> _hlAnsiList = new HighlightedAnsiList();

        private ObservableCollection<AnsiColor> _standardAnsiFG = new StandardAnsiList();
        public ObservableCollection<AnsiColor> StandardAnsiFG
        {
            get
            {
                if (_standardAnsiFG == null)
                {
                    _standardAnsiFG = _ansiList;
                }
                return _standardAnsiFG;
            }
            set
            {
                if (_hFG == false)
                {
                    _standardAnsiFG = _ansiList;
                }
                else
                {
                    _standardAnsiFG = _hlAnsiList;
                }
            }
        }

        private ObservableCollection<AnsiColor> _standardAnsiBG = new StandardAnsiList();
        public ObservableCollection<AnsiColor> StandardAnsiBG
        {
            get
            {
                if (_standardAnsiBG == null)
                {
                    _standardAnsiBG = _ansiList;
                }
                return _standardAnsiBG;
            }
            set
            {
                if (_hBG == false)
                {
                    _standardAnsiBG = _ansiList;
                }
                else
                {
                    _standardAnsiBG = _hlAnsiList;
                }
            }
        }

        private bool _hFG = false;
        public bool HFG
        {
            get { return _hFG; }
            set 
            {
                _hFG = value;
                RaisePropertyChanged("HFG");
                if (value == true)
                {
                    _standardAnsiFG = _hlAnsiList;
                    _selectedFG = null;
                }
                else
                {
                    _standardAnsiFG = _ansiList;
                    _selectedFG = null;
                
                }
                RaisePropertyChanged("StandardAnsiFG");
                RaisePropertyChanged("SelectedFG");
            }
        }

        private bool _hBG = false;
        public bool HBG
        {
            get { return _hBG; }
            set
            {
                _hBG = value;
                RaisePropertyChanged("HBG");
                if (value == true)
                {
                    _standardAnsiBG = _hlAnsiList;
                    _selectedBG = null;
                }
                else
                {
                    _standardAnsiBG = _ansiList;
                    _selectedBG = null;
                }
                RaisePropertyChanged("StandardAnsiBG");
                RaisePropertyChanged("SelectedBG");
            }
        }

        private AnsiColor _selectedFG;
        public AnsiColor SelectedFG
        {
            get { return _selectedFG; }
            set
            {
                _selectedFG = value;
                RaisePropertyChanged("SelectedFG");
                if (_selectedFG != null && _selectedBG !=null)
                { 
                    _ansiCodes = _selectedFG.ColorName + _selectedBG.ColorName;
                }
                else if (_selectedFG !=null && _selectedBG == null)
                {
                    _ansiCodes = _selectedFG.ColorName;
                }
                else
                {
                    _ansiCodes = "w";
                }
            }
        }

        private AnsiColor _selectedBG;
        public AnsiColor SelectedBG
        {
            get { return _selectedBG; }
            set
            {
                _selectedBG = value;
                RaisePropertyChanged("SelectedBG");
                if (_selectedFG != null && _selectedBG != null)
                {
                    _ansiCodes = _selectedFG.ColorName + _selectedBG.ColorName;
                }
                else if (_selectedBG != null && _selectedFG == null)
                {
                    _ansiCodes = _selectedFG.ColorName;
                }
                else
                {
                    _ansiCodes = "w";
                }
            }
        }

        private bool _spaceOnOff;
        public bool SpaceOnOff
        {
            get { return _spaceOnOff; }
            set
            {
                _spaceOnOff = value;
                RaisePropertyChanged("SpaceOnOff");

                if (value == true)
                {
                    _text = _text.Replace(" ", "%b");
                    
                }
                else
                {
                    _text = _text.Replace("%b", " ");
                }

                RaisePropertyChanged("Text");
            }
        }

        private bool _returnOnOff;
        public bool ReturnOnOff
        {
            get { return _returnOnOff; }
            set
            {
                _returnOnOff = value;
                RaisePropertyChanged("ReturnOnOff");

                if (value == true)
                {
                    //_text = _text.Replace("\r", "%r");
                    _text = _text.Replace("\r\n", "%r");

                }
                else
                {
                    _text = _text.Replace("%r", "\r\n");
                }

                RaisePropertyChanged("Text");
            }
        }

        private bool _menuFlyOutState;
        public bool MenuFlyOutState
        {
            get
            {
                return _menuFlyOutState;
            }
            set
            {
                _menuFlyOutState = value;
                RaisePropertyChanged("MenuFlyOutState");
            }
        }

        private bool _colorPickerFlyOutState;
        public bool ColorPickerFlyOutState
        {
            get
            {
                return _colorPickerFlyOutState;
            }
            set
            {
                _colorPickerFlyOutState = value;
                RaisePropertyChanged("ColorPickerFlyOutState");
            }
        }

        #endregion


    }
}