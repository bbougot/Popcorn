using System.Windows;
using System.Windows.Input;

namespace Popcorn.AttachedProperties
{
    public static class MouseCommandBehavior
    {
        #region Commands

        ///
        /// The comamnd which should be executed when the mouse is down
        ///
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseDownCommand",
                typeof(ICommand),
                typeof(MouseCommandBehavior),
                new FrameworkPropertyMetadata(null,
                    (obj, e) => OnMouseCommandChanged(obj, (ICommand) e.NewValue, false)));

        ///
        /// Gets the MouseDownCommand property
        ///
        public static ICommand GetMouseDownCommand(DependencyObject d)
        {
            return (ICommand) d.GetValue(MouseDownCommandProperty);
        }

        ///
        /// Sets the MouseDownCommand property
        ///
        public static void SetMouseDownCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(MouseDownCommandProperty, value);
        }

        ///
        /// The comamnd which should be executed when the mouse is up
        ///
        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseUpCommand",
                typeof(ICommand),
                typeof(MouseCommandBehavior),
                new FrameworkPropertyMetadata(null,
                    (obj, e) => OnMouseCommandChanged(obj, (ICommand) e.NewValue, true)));

        ///
        /// Gets the MouseUpCommand property
        ///
        public static ICommand GetMouseUpCommand(DependencyObject d)
        {
            return (ICommand) d.GetValue(MouseUpCommandProperty);
        }

        ///
        /// Sets the MouseUpCommand property
        ///
        public static void SetMouseUpCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(MouseUpCommandProperty, value);
        }

        #endregion

        ///
        /// Registeres the event and calls the command when it gets fired
        ///
        private static void OnMouseCommandChanged(DependencyObject d, ICommand command, bool isMouseUp)
        {
            if (command == null) return;

            var element = (FrameworkElement) d;

            if (isMouseUp)
                element.PreviewMouseUp += (obj, e) => command.Execute(null);
            else
                element.PreviewMouseDown += (obj, e) => command.Execute(null);
        }
    }
}