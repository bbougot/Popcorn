using System.Windows;
using System.Windows.Controls;

namespace Popcorn.AttachedProperties
{
    /// <summary>
    /// Enable drag for a control
    /// </summary>
    public static class TextParenthesis
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(TextParenthesis),
            new PropertyMetadata(default(string), OnLoaded));

        private static void OnLoaded(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBlock = dependencyObject as TextBlock;
            if (textBlock == null) return;

            var text = dependencyPropertyChangedEventArgs.NewValue as string;
            if (text == null) return;

            textBlock.Text = $"({text})";
        }

        public static void SetText(DependencyObject element, bool value)
        {
            element.SetValue(TextProperty, value);
        }

        public static string GetText(DependencyObject element)
        {
            return (string) element.GetValue(TextProperty);
        }
    }
}