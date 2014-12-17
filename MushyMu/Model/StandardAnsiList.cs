using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MushyMu.Model
{
    class StandardAnsiList : ObservableCollection<AnsiColor>
    {
        public StandardAnsiList() : base()
        {
            Add(new AnsiColor("Black", 30, false, new SolidColorBrush(Color.FromRgb(54, 54, 54))));
            Add(new AnsiColor("Red", 31, false, new SolidColorBrush(Color.FromRgb(128, 0, 0))));
            Add(new AnsiColor("Green", 32, false, new SolidColorBrush(Color.FromRgb(0, 128, 0))));
            Add(new AnsiColor("Yellow", 33, false, new SolidColorBrush(Color.FromRgb(128, 128, 0))));
            Add(new AnsiColor("Blue", 34, false, new SolidColorBrush(Color.FromRgb(0, 0, 128))));
            Add(new AnsiColor("Magenta", 35, false, new SolidColorBrush(Color.FromRgb(128, 0, 128))));
            Add(new AnsiColor("Cyan", 36, false, new SolidColorBrush(Color.FromRgb(0, 128, 128))));
            Add(new AnsiColor("White", 37, false, new SolidColorBrush(Color.FromRgb(229, 229, 229))));
        }
    }
}
