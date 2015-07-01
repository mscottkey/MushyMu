using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MushyMu.Controls
{
    public class ScrollViewerHelper : Behavior<ScrollViewer>
    {
        public object UpdateTrigger
        {
            get { return (object)GetValue(UpdateTriggerProperty); }
            set { SetValue(UpdateTriggerProperty, value); }
        }

        public static readonly DependencyProperty UpdateTriggerProperty =
            DependencyProperty.Register("UpdateTrigger", typeof(object), typeof(ScrollViewerHelper), new UIPropertyMetadata(Update));

        private bool IsScrolledDown
        {
            get { return (bool)GetValue(IsScrolledDownProperty); }
            set { SetValue(IsScrolledDownProperty, value); }
        }

        public static readonly DependencyProperty IsScrolledDownProperty =
            DependencyProperty.Register("IsScrolledDown", typeof(bool), typeof(ScrollViewerHelper), new UIPropertyMetadata(false));

        private static void Update(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)d.GetValue(IsScrolledDownProperty))
            {
                var scroll = ((ScrollViewerHelper)d).AssociatedObject;
                scroll.ScrollToEnd();
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += new RoutedEventHandler(AssociatedObject_Loaded);
            AssociatedObject.ScrollChanged += new ScrollChangedEventHandler(AssociatedObject_ScrollChanged);
        }
        
        private void AssociatedObject_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            IsScrolledDown = AssociatedObject.VerticalOffset == AssociatedObject.ScrollableHeight;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            IsScrolledDown = AssociatedObject.VerticalOffset == AssociatedObject.ScrollableHeight;
        }
    }
}
