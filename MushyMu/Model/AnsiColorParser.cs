using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace MushyMu.Model
{
    public struct AnsiTextRun
    {
        public string Content;
        public SolidColorBrush ForegroundColor;
        public SolidColorBrush BackgroundColor;
        //public bool UnderLined;

        
       
    }


    //scans incoming text for ANSI control sequences to provide a stream of styled text for end-user viewing
    class ANSIColorParser
    {
        //regular expression to fit ANSI control sequences
        public readonly string ansiControlRegEx = (char)27 + @"\[" + "[^@-~]*" + "[@-~]";


        //current settings (with defaults)

        private SolidColorBrush foregroundColor = Brushes.LightGray;
        private SolidColorBrush backgroundColor = null;
        //private bool underLined = false;
        private bool brightColors = true;
        //Setup Values to build on if Ansi Codes are stacked.
        public int fgColorCode;
        public int bgColorCode;
        public SolidColorBrush fgColor;
        public SolidColorBrush bgColor;
        public bool isHighlighted;
        public bool isInverted;

        //scans incoming text for ANSI control sequences, parses them, and returns a list of styled text runs
        public List<AnsiTextRun> Parse(string text)
        {
            //System.Diagnostics.Debug.WriteLine(text);
            //start with an empty list of runs
            List<AnsiTextRun> returnRuns = new List<AnsiTextRun>();

            //argument validation
            if (string.IsNullOrEmpty(text))
                return returnRuns;

            //in simplest case, the generated run will represent the entire string
            int runStartIndex = 0;
            int runEndIndex = text.Length - 1;

            //find all the control sequences
            MatchCollection matches = Regex.Matches(text, this.ansiControlRegEx);

            //for each control sequence
            foreach (Match match in matches)
            {
                string matchValue = match.Value;

                //identify the operation by grabbing the last character
                char operationToken = matchValue[matchValue.Length - 1];

                //identify the arguments by splitting on semicolon, then converting to integers
                string argsString = matchValue.Substring(2, matchValue.Length - 3);
                string[] argsArray = argsString.Split(';');
                List<int> arguments = new List<int>();

                foreach (string argument in argsArray)
                {
                    try
                    {
                        arguments.Add(int.Parse(argument));
                    }

                    //if we can't convert to an integer, it's a badly formed parameter and we'll ignore it
                    catch (FormatException) { }
                    catch (OverflowException) { }
                }

                //now apply whatever changes are necessary according to the operation and parameters

                //'m' is "select graphics rendition", and indicates a style change
                //we need a new run for each continuous string of same-style characters
                //so then, ending the current run and starting a new one
                if (operationToken.Equals('m'))
                {
                    //figure out start index, end index, and text of the completed run (remember runStartIndex initialized to zero)
                    //last run ends just before we encountered the ANSI control sequence
                    runEndIndex = match.Index - 1;
                    string runText = text.Substring(runStartIndex, runEndIndex - runStartIndex + 1);

                    //build a run out of the current color settings and text up until this point
                    AnsiTextRun newRun;
                    newRun.Content = runText;
                    newRun.ForegroundColor = this.foregroundColor;
                    newRun.BackgroundColor = this.backgroundColor;
                    //newRun.UnderLined = this.underLined;

                    returnRuns.Add(newRun);

                    //note the new start index of the next run, which will start after the end of the control sequence
                    runStartIndex = match.Index + match.Length;

                    //assume the next run will continue until the end of the string, until proven otherwise
                    runEndIndex = text.Length - 1;


                    //Finally call up the color list
                    AnsiColorDictionary colorList = new AnsiColorDictionary();

                    int i = 0;
                    //parameters will determine the style of the next run, and there may be several parameters
                    foreach (int param in arguments)
                    {
                        //Increase counter
                        i++;
                        //reset to defaults
                        if (param == 0)
                        {
                            this.isHighlighted = false;
                            this.isInverted = false;
                            this.fgColor = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                            this.bgColor = Brushes.Black;
                            this.fgColorCode = 0;
                            this.bgColorCode = 0;
                        }

                        ////Start with Bold, Underline, Italics
                        //if (param == 4) // Underline
                        //{
                        //    this.underLined = true;
                        //}

                        //bright colors on
                        //if (param == 1)
                        //{
                        //    this.brightColors = true;
                        //    this.isHighlighted = true;
                        //}

                        ////bright colors off
                        //else if (param == 22)
                        //{
                        //    this.brightColors = false;
                        //    this.isHighlighted = false;
                        //}

                        else if (param >= 30 && param <= 37)
                        {
                            if (i == 1 || i == 2)
                            {
                                if (isHighlighted == true)
                                {
                                    if (isInverted == true)
                                    {
                                        //Highlighted and inverted
                                        bgColorCode = colorList[param + 1000].ColorCode;
                                        bgColor = colorList[param + 1000].RGB;

                                    }
                                    else
                                    {
                                        //Highlighted foreground color not inverted
                                        fgColorCode = colorList[param + 1000].ColorCode;
                                        fgColor = colorList[param + 1000].RGB;
                                    }
                                }

                                else if (isInverted == true)
                                {
                                    //Inverted and not highlighted
                                    bgColorCode = colorList[param].ColorCode;
                                    bgColor = colorList[param].RGB;
                                }
                                else
                                {
                                    //Regular foreground color
                                    fgColorCode = colorList[param].ColorCode;
                                    fgColor = colorList[param].RGB;
                                }
                            }
                        }
                        else if (param >= 40 && param <= 47)
                        {
                            if (i == 1 || i == 2)
                            {
                                if (isHighlighted == true)
                                {
                                    if (isInverted == true)
                                    {
                                        //Highlighted and inverted
                                        fgColorCode = colorList[param + 1000].ColorCode;
                                        fgColor = colorList[param + 1000].RGB;

                                    }
                                    else
                                    {
                                        //Highlighted foreground color not inverted
                                        bgColorCode = colorList[param + 1000].ColorCode;
                                        bgColor = colorList[param + 1000].RGB;
                                    }
                                }
                                else if (isInverted == true)
                                {
                                    //Inverted and not highlighted
                                    fgColorCode = colorList[param].ColorCode;
                                    fgColor = colorList[param].RGB;
                                }
                                else
                                {
                                    //Regular foreground color
                                    bgColorCode = colorList[param].ColorCode;
                                    bgColor = colorList[param].RGB;
                                }
                            }
                        }

                        else if (param == 1)
                        {
                            isHighlighted = true;
                            if (fgColorCode > 0 && fgColorCode != 38 && fgColorCode != 48)
                            {
                                fgColor = colorList[fgColorCode + 1000].RGB;
                            }
                        }

                        else if (param == 22)
                        {
                            isHighlighted = false;
                        }

                        //Code 7 = Inverted colors
                        else if (param == 7)
                        {
                            isInverted = true;
                        }
                        
                        //default background color
                        else if (param == 49)
                        {
                            this.backgroundColor = Brushes.Black;
                            if (this.brightColors) this.backgroundColor = Brushes.DarkGray;
                        }

                        //default foreground color
                        else if (param == 39)
                        {
                            this.foregroundColor = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                            if (this.brightColors) this.foregroundColor = Brushes.White;
                        }

                        //handle xterm foreground here
                        else if (param == 38)
                        {
                            fgColorCode = 38;
                            int xterm = arguments[2];
                            fgColor = colorList[2000 + xterm].RGB;
                            
                        }

                        else if (param == 48)
                        {
                            bgColorCode = 48;
                            int xterm = arguments[2];
                            bgColor = colorList[2000 + xterm].RGB;
                        }
                            
                    }
                    this.foregroundColor = fgColor;
                    this.backgroundColor = bgColor;
                }

                //if the ansi control sequence has an unsupported operation code, show it in the UI highlighted in a bright color
                else
                {
                    AnsiTextRun controlSequenceRun;
                    controlSequenceRun.Content = matchValue;
                    controlSequenceRun.BackgroundColor = Brushes.Yellow;
                    controlSequenceRun.ForegroundColor = Brushes.Black;
                    returnRuns.Add(controlSequenceRun);
                }
            }

            //now that we're done with the last control sequence, build a run from any remaining ("trailing") text

            //if there were no control sequences at all, the "trailing text" would be the entire string
            int trailingTextStartIndex = 0;

            //if there were control sequences, the trailing text would start after the last control sequence ended
            if (matches.Count > 0)
            {
                //get the last control sequence
                Match lastMatch = matches[matches.Count - 1];

                //figure out where it ends
                trailingTextStartIndex = lastMatch.Index + lastMatch.Length;
            }

            //if there's any trailing text, build a run from that text using current style
            if (trailingTextStartIndex < text.Length - 1)
            {
                AnsiTextRun trailingRun;
                trailingRun.Content = text.Substring(trailingTextStartIndex);
                trailingRun.ForegroundColor = this.foregroundColor;
                trailingRun.BackgroundColor = this.backgroundColor;
                returnRuns.Add(trailingRun);
            }

            //ConvertUrlsToLinks(returnRuns);
            
            return returnRuns;
        }
    }
}
