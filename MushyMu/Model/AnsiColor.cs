using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MushyMu.Model
{
    public class AnsiColor
    {
        private string name;
        private int code;
        private bool isHighlighted;
        private SolidColorBrush rgb;

        public AnsiColor(string ColorName, int ColorCode, bool IsHighlighted, SolidColorBrush RGB)
        {
            this.name = ColorName;
            this.code = ColorCode;
            this.isHighlighted = IsHighlighted;
            this.rgb = RGB;
            
        }

        public string ColorName
        {
            get
            { return name; }
            set
            {
                name = value;
            }
        }
        public int ColorCode
        {
            get { return code; }
            set
            {
                code = value;
            }
        }
        public bool IsHighlighted 
        {
            get
            { return isHighlighted; }
                
            set
            {
                isHighlighted = value;
            }
        
        }
        public SolidColorBrush RGB
        {
            get
            {
                return rgb;
            }
            set
            {
                rgb = value;
            }
        }
    }
}
