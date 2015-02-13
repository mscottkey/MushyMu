using System.Windows;
using MushyMu.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MushyMu.Controls;
using System.Xml;
using System.Linq;
using MushyMu.Views;
using System.Windows.Input;

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
            Messenger.Default.Register<GameViewModel>(this, "OpenTextEditor", (_gameVM) => ShowTextEditor(_gameVM));
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
                    case "ResetInputHistoryIndex":
                        ResetInputHistoryIndex();
                        break;
                    case "InvalidConnectInfo":
                        InvalidConnectInfo();
                        break;
                    case "FlashWindow":
                        ActiveFlashWindow();
                        break;
                    

                    default:
                        break;

                }
            });
        }

        private void ResetInputHistoryIndex()
        {
            //lbxInputHistory.SelectedIndex = -1;
            lbxInputHistory.UnselectAll();
        }

        private async void InvalidConnectInfo()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Ok",
                //NegativeButtonText = "Go away!",
                //FirstAuxiliaryButtonText = "Cancel",
                //ColorScheme = UseAccentForDialogsMenuItem.IsChecked ? MetroDialogColorScheme.Accented : MetroDialogColorScheme.Theme
            };

            MessageDialogResult result = await this.ShowMessageAsync("Error", "Either the host and/or port provided are incorrect, or that game is currently down. Please try again.",
                MessageDialogStyle.Affirmative, mySettings);
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


        private void ActiveFlashWindow()
        {
            var helper = new FlashWindowHelper(Application.Current);
            helper.FlashApplicationWindow();

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Grab some user settings from the Settings XML

            //Define app level XML variables
            XmlDocument settingsXML = new XmlDocument();
            settingsXML.Load(@"C:\ProgramData\MushyMu\MushyMuSettings.xml");

            XmlNode settings = settingsXML.SelectSingleNode("//settings");

            //Get Default Accent Color
            XmlNode colorNode = settings.SelectSingleNode("color");

            Accent _accent;
            AppTheme _theme = ThemeManager.AppThemes.First(x => x.Name == "BaseDark"); 

            if (!String.IsNullOrEmpty(colorNode.InnerText))
            {
                _accent = ThemeManager.Accents.First(x => x.Name == colorNode.InnerText);
            }
            else
            {
                _accent = ThemeManager.Accents.First(x => x.Name == "Steel");
            }

            ThemeManager.ChangeAppStyle(App.Current, _accent, _theme);

        }

        private void ShowTextEditor(GameViewModel vm)
        {
            var textEditor = new TextEditorView(vm);
            textEditor.Show();
        }


    }
}