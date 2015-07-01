using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MushyMu.Controls
{
    /// <summary>
    /// Overall class for the output box.
    /// </summary>
    public class MuBox : FlowDocumentScrollViewer
    {
        /// <summary>
        /// Backing store for the <see cref="ScrollViewer"/> property.
        /// </summary>
        private ScrollViewer scrollViewer;

        /// <summary>
        /// Gets the scroll viewer contained within the FlowDocumentScrollViewer control
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get
            {
                if (this.scrollViewer == null)
                {
                    DependencyObject obj = this;

                    do
                    {
                        if (VisualTreeHelper.GetChildrenCount(obj) > 0)
                            obj = VisualTreeHelper.GetChild(obj as Visual, 0);
                        else
                            return null;
                    }
                    while (!(obj is ScrollViewer));

                    this.scrollViewer = obj as ScrollViewer;
                }

                return this.scrollViewer;
            }
        }

    }
}
