using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MushyMu.Controls
{
    /// <summary>
    /// Interaction logic for FontSelector.xaml
    /// </summary>
    public partial class FontSelector : UserControl
    {
        public FontSelector()
        {
            InitializeComponent();
        }

        private int _selectedFont;
        public int SelectedFont
        {
            get { return _selectedFont; }
            set
            {
                _selectedFont = value;
            }
        }
    }
}
