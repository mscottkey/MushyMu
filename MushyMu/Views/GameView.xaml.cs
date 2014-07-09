using GalaSoft.MvvmLight.Messaging;
using MushyMu.Model;
using MushyMu.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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



        public GameView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                switch (message.Notification)
                {
                    case "ResetTextBoxFocus":
                        ResetTextBoxFocus();
                        break;
                    
                    default:
                        break;
                }
            });
            
        }

        private void ResetTextBoxFocus()
        {
            Keyboard.Focus(tbInputArea);
            tbInputArea.Select(tbInputArea.Text.Length, 0);
        }



        private void fdrOutputArea_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            fdrOutputArea.ScrollViewer.UpdateLayout();
            fdrOutputArea.ScrollViewer.ScrollToEnd();
        }

        private void tbInputArea_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(tbInputArea);
        }

       
    }
}