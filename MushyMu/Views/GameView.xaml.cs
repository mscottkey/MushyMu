using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using MushyMu.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MushyMu.Views
{
    /// <summary>
    /// Description for NewGameView.
    /// </summary>
    public partial class GameView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the NewGameView class.
        /// </summary>

        private bool AutoScroll;

        public GameView()
        {
            InitializeComponent();
            AutoScroll = true;
            
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                switch (message.Notification)
                {
                    case "ResetTextBoxFocus":
                        ResetTextBoxFocus();
                        break;
                    case "PageUpCommand":
                        PageUp();
                        break;
                    case "PageDownCommand":
                        PageDown();
                        break;
                    default:
                        break;
                }
            });

        }

        private void PageUp()
        {
            MuScroll.PageUp();
        }

        private void PageDown()
        {
            MuScroll.PageDown();
        }

        private void ResetTextBoxFocus()
        {
            Keyboard.Focus(tbInputArea);
            tbInputArea.Select(tbInputArea.Text.Length, 0);
            tbInputArea.Text = String.Empty;
            MuScroll.UpdateLayout();
            MuScroll.ScrollToEnd();
        }


        private void RTBOutputArea_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {

            if (AutoScroll == true)
            {

                MuScroll.ScrollToEnd();
                MuScroll.UpdateLayout();
            }


        }

        

        private void tbInputArea_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(tbInputArea);
        }

        

        private void MuScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (MuScroll.VerticalOffset == MuScroll.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                MuScroll.ScrollToVerticalOffset(MuScroll.ExtentHeight);
                MuScroll.UpdateLayout();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            DoSearch(RTBOutputArea, tbSearch.Text, false);

        }


        public bool DoSearch(RichTextBox richTextBox, string searchText, bool searchNext)
        {
            TextRange searchRange;

            // Get the range to search
            if (searchNext)
                searchRange = new TextRange(
                    richTextBox.Selection.Start.GetPositionAtOffset(1),
                    richTextBox.Document.ContentEnd);
            else
                searchRange = new TextRange(
                    richTextBox.Document.ContentStart,
                    richTextBox.Document.ContentEnd);

            // Do the search
            TextRange foundRange = FindTextInRange(searchRange, searchText);
            if (foundRange == null)
                return false;

            // Select the found range
            richTextBox.Selection.Select(foundRange.Start, foundRange.End);
            return true;
        }

        public TextRange FindTextInRange(TextRange searchRange, string searchText)
        {
            // Search the text with IndexOf
            int offset = searchRange.Text.IndexOf(searchText);
            if(offset<0)
                return null;  // Not found

            // Try to select the text as a contiguous range
            for(TextPointer start = searchRange.Start.GetPositionAtOffset(offset); start != searchRange.End; start = start.GetPositionAtOffset(2))
            {
                TextRange result = new TextRange(start, start.GetPositionAtOffset(searchText.Length));
                if(result.Text == searchText)
                return result;
            }
            return null;
          }

       
    }
}