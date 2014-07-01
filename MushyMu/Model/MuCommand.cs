using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushyMu.Model
{
    public class MuCommand
    {
        private string name;
        private int iD;
        private bool submitOnSelect;

        public MuCommand(string CmdName, int CmdID, bool CmdSubmit)
        {
            this.name = CmdName;
            this.iD = CmdID;
            this.submitOnSelect = CmdSubmit;
            
        }

        public string Name
        {
            get
            { return name; }
            set
            {
                name = value;
            }
        }
        public int ID
        {
            get { return iD; }
            set
            {
                iD = value;
            }
        }
        public bool SubmitOnSelect 
        {
            get
            { return submitOnSelect; }
                
            set
            {
                submitOnSelect = value;
            }
        
        }
    }
}
