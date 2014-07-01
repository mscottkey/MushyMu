using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushyMu.Model
{
    public class CommandList : ObservableCollection<MuCommand>
    {
        public CommandList() : base()
        {
            Add(new MuCommand("WHO", 0, true));
            Add(new MuCommand("+where", 1, true));
            Add(new MuCommand("+pub ", 2, false));
            Add(new MuCommand("+new ", 3, false));
            Add(new MuCommand("say ", 4, false));
            Add(new MuCommand("pose ", 5, false));
            Add(new MuCommand("@emit ", 6, false));
        }
    }
}
