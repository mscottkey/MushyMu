using MahApps.Metro.Controls;
using MushyMu.ViewModel;
using System.Windows;

namespace MushyMu.Views
{
    /// <summary>
    /// Description for TextEditorView.
    /// </summary>
    public partial class TextEditorView : MetroWindow
    {
        TextEditorViewModel textEditorVM;

        /// <summary>
        /// Initializes a new instance of the TextEditorView class.
        /// </summary>
        public TextEditorView(GameViewModel vm)
        {
            InitializeComponent();
            textEditorVM = new TextEditorViewModel(vm);
            this.DataContext = textEditorVM;
        }
    }
}