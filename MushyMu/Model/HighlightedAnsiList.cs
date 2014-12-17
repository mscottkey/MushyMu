using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MushyMu.Model
{
    class HighlightedAnsiList : ObservableCollection<AnsiColor>
    {
        public HighlightedAnsiList() : base()
        {
            Add(new AnsiColor("Black", 30, true, new SolidColorBrush(Color.FromRgb(128, 128, 128))));
            Add(new AnsiColor("Red", 31, true, new SolidColorBrush(Color.FromRgb(255, 0, 0))));
            Add(new AnsiColor("Green", 32, true, new SolidColorBrush(Color.FromRgb(0, 255, 0))));
            Add(new AnsiColor("Yellow", 33, true, new SolidColorBrush(Color.FromRgb(255, 255, 0))));
            Add(new AnsiColor("Blue", 34, true, new SolidColorBrush(Color.FromRgb(0, 0, 255))));
            Add(new AnsiColor("Magenta", 35, true, new SolidColorBrush(Color.FromRgb(255, 0, 255))));
            Add(new AnsiColor("Cyan", 36, true, new SolidColorBrush(Color.FromRgb(0, 255, 255))));
            Add(new AnsiColor("White", 37, true, new SolidColorBrush(Color.FromRgb(255, 255, 255))));
        }
    }
}
