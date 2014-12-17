using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushyMu.Model
{
    public class ThemeColorList : ObservableCollection<Accent>
    {
        public ThemeColorList() : base()
        {
            List<Accent> colorList = ThemeManager.Accents.ToList<Accent>();

            foreach(Accent c in  colorList)
            {
                Add(c);
            }
            
        }
    }
}
