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
            
        }

        private void fdrOutputArea_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            ScrollBar.ScrollToEnd();
        }

        private void tbInputArea_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(tbInputArea);
        }
    }
}