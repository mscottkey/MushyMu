using System.Windows;
using MushyMu.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using System;

namespace MushyMu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                switch (message.Notification)
                {
                    case "DialogIncompleteConnectionInformation":
                        ShowIncompleteDialog();
                        break;
                    case "DialogSaveComplete":
                        ShowSaveCompleteDialog();
                        break;
                    case "ResetSelectedCommand":
                        ResetSelectedCommand();
                        break;

                    default:
                        break;

                }
            });
        }

        private void ResetSelectedCommand()
        {
            lbxCommonCmds.UnselectAll();
        }

        private async void ShowSaveCompleteDialog()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Ok",
                //NegativeButtonText = "Go away!",
                //FirstAuxiliaryButtonText = "Cancel",
                //ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
            };

            MessageDialogResult result = await this.ShowMessageAsync("Saved", "Your new game has been saved.",
                MessageDialogStyle.Affirmative, mySettings);


 
        }

        private async void ShowIncompleteDialog()
        {
           // MetroDialogOptions.ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme;

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Ok",
                //NegativeButtonText = "Go away!",
                //FirstAuxiliaryButtonText = "Cancel",
                //ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
            };

            MessageDialogResult result = await this.ShowMessageAsync("Failed to Save", "You need to fill out Name, Host and Port to save.",
                MessageDialogStyle.Affirmative, mySettings);

            //if (result != MessageDialogResult.FirstAuxiliary)
            //    await this.ShowMessageAsync("Result", "You said: " + (result == MessageDialogResult.Affirmative ? mySettings.AffirmativeButtonText : mySettings.NegativeButtonText +
            //        Environment.NewLine + Environment.NewLine + "This dialog will follow the Use Accent setting."));
        }
        
    }
}