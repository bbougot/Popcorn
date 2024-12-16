using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Popcorn.AttachedProperties
{
    /// <summary>
    /// Attached properties for persistent tab control
    /// </summary>
    /// <remarks>
    /// By default WPF TabControl bound to an ItemsSource destroys visual state of invisible tabs.
    /// Set Helpers:TabContent.IsCached="True" to preserve visual state of each tab.
    /// </remarks>
    public static class TabContent
    {
        /// <summary>
        /// Controls whether tab content is cached or not
        /// </summary>
        /// <remarks>When TabContent.IsCached is true, visual state of each tab is preserved (cached), even when the tab is hidden</remarks>
        public static readonly DependencyProperty IsCachedProperty =
            DependencyProperty.RegisterAttached("IsCached", typeof(bool), typeof(TabContent),
                new UIPropertyMetadata(false, OnIsCachedChanged));

        /// <summary>
        /// Used instead of TabControl.ContentTemplate for cached tabs
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(DataTemplate), typeof(TabContent),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Used instead of TabControl.ContentTemplateSelector for cached tabs
        /// </summary>
        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(TabContent),
                new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for InternalTabControl.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty
            InternalTabControlProperty =
                DependencyProperty.RegisterAttached("InternalTabControl", typeof(TabControl), typeof(TabContent),
                    new UIPropertyMetadata(null, OnInternalTabControlChanged));

        // Using a DependencyProperty as the backing store for InternalCachedContent.  This enables animation, styling, binding, etc...
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty
            InternalCachedContentProperty =
                DependencyProperty.RegisterAttached("InternalCachedContent", typeof(ContentControl),
                    typeof(TabContent), new UIPropertyMetadata(null));

        // Using a DependencyProperty as the backing store for InternalContentManager.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InternalContentManagerProperty =
            DependencyProperty.RegisterAttached("InternalContentManager", typeof(object), typeof(TabContent),
                new UIPropertyMetadata(null));

        public static bool GetIsCached(DependencyObject obj) => (bool) obj.GetValue(IsCachedProperty);

        public static void SetIsCached(DependencyObject obj, bool value) => obj.SetValue(IsCachedProperty, value);

        private static DataTemplate GetTemplate(DependencyObject obj) => (DataTemplate) obj.GetValue(TemplateProperty);

        public static void SetTemplate(DependencyObject obj, DataTemplate value)
            => obj.SetValue(TemplateProperty, value);


        private static DataTemplateSelector GetTemplateSelector(DependencyObject obj)
            => (DataTemplateSelector) obj.GetValue(TemplateSelectorProperty);

        public static void SetTemplateSelector(DependencyObject obj, DataTemplateSelector value)
            => obj.SetValue(TemplateSelectorProperty, value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TabControl GetInternalTabControl(DependencyObject obj)
            => (TabControl) obj.GetValue(InternalTabControlProperty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetInternalTabControl(DependencyObject obj, TabControl value)
            => obj.SetValue(InternalTabControlProperty, value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static ContentControl GetInternalCachedContent(DependencyObject obj)
            => (ContentControl) obj.GetValue(InternalCachedContentProperty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void SetInternalCachedContent(DependencyObject obj, ContentControl value)
            => obj.SetValue(InternalCachedContentProperty, value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static object GetInternalContentManager(DependencyObject obj)
            => obj.GetValue(InternalContentManagerProperty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void SetInternalContentManager(DependencyObject obj, object value)
            => obj.SetValue(InternalContentManagerProperty, value);

        private static void OnIsCachedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj == null) return;

            var tabControl = obj as TabControl;
            if (tabControl == null)
                throw new InvalidOperationException("Cannot set TabContent.IsCached on object of type " +
                                                    args.NewValue.GetType().Name +
                                                    ". Only objects of type TabControl can have TabContent.IsCached property.");

            var newValue = (bool) args.NewValue;

            if (!newValue)
            {
                if (args.OldValue != null && ((bool) args.OldValue))
                    throw new NotImplementedException(
                        "Cannot change TabContent.IsCached from True to False. Turning tab caching off is not implemented");

                return;
            }

            EnsureContentTemplateIsNull(tabControl);
            tabControl.ContentTemplate = CreateContentTemplate();
            EnsureContentTemplateIsNotModified(tabControl);
        }

        private static DataTemplate CreateContentTemplate()
        {
            const string xaml =
                "<DataTemplate><Border b:TabContent.InternalTabControl=\"{Binding RelativeSource={RelativeSource AncestorType=TabControl}}\" /></DataTemplate>";

            var context = new ParserContext {XamlTypeMapper = new XamlTypeMapper(new string[0])};

            context.XamlTypeMapper.AddMappingProcessingInstruction("b", typeof(TabContent).Namespace,
                typeof(TabContent).Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("b", "b");

            var template = (DataTemplate) XamlReader.Parse(xaml, context);
            return template;
        }

        private static void OnInternalTabControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj == null) return;
            var container = obj as Decorator;

            if (container == null)
            {
                var message = "Cannot set TabContent.InternalTabControl on object of type " + obj.GetType().Name +
                              ". Only controls that derive from Decorator, such as Border can have a TabContent.InternalTabControl.";
                throw new InvalidOperationException(message);
            }

            if (args.NewValue == null) return;
            if (!(args.NewValue is TabControl))
                throw new InvalidOperationException("Value of TabContent.InternalTabControl cannot be of type " +
                                                    args.NewValue.GetType().Name + ", it must be of type TabControl");

            var tabControl = (TabControl) args.NewValue;
            var contentManager = GetContentManager(tabControl, container);
            contentManager.UpdateSelectedTab();
        }

        private static ContentManager GetContentManager(TabControl tabControl, Decorator container)
        {
            var contentManager = (ContentManager) GetInternalContentManager(tabControl);
            if (contentManager != null)
                /*
                 * Content manager already exists for the tab control. This means that tab content template is applied 
                 * again, and new instance of the Border control (container) has been created. The old container 
                 * referenced by the content manager is no longer visible and needs to be replaced
                 */
                contentManager.ReplaceContainer(container);
            else
            {
                // create content manager for the first time
                contentManager = new ContentManager(tabControl, container);
                SetInternalContentManager(tabControl, contentManager);
            }

            return contentManager;
        }

        private static void EnsureContentTemplateIsNull(TabControl tabControl)
        {
            if (tabControl.ContentTemplate != null)
                throw new InvalidOperationException(
                    "TabControl.ContentTemplate value is not null. If TabContent.IsCached is True, use TabContent.Template instead of ContentTemplate");
        }

        private static void EnsureContentTemplateIsNotModified(TabControl tabControl)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(TabControl.ContentTemplateProperty,
                typeof(TabControl));
            descriptor.AddValueChanged(tabControl, (sender, args) =>
            {
                throw new InvalidOperationException(
                    "Cannot assign to TabControl.ContentTemplate when TabContent.IsCached is True. Use TabContent.Template instead");
            });
        }

        private class ContentManager
        {
            private TabControl TabControl { get; }
            private Decorator _border;

            public ContentManager(TabControl tabControl, Decorator border)
            {
                TabControl = tabControl;
                _border = border;
                TabControl.SelectionChanged += (sender, args) => { UpdateSelectedTab(); };
            }

            public void ReplaceContainer(Decorator newBorder)
            {
                if (ReferenceEquals(_border, newBorder)) return;

                _border.Child = null; // detach any tab content that old border may hold
                _border = newBorder;
            }

            public void UpdateSelectedTab() => _border.Child = GetCurrentContent();

            private ContentControl GetCurrentContent()
            {
                var item = TabControl.SelectedItem;
                if (item == null) return null;

                var tabItem = TabControl.ItemContainerGenerator.ContainerFromItem(item);
                if (tabItem == null) return null;

                var cachedContent = GetInternalCachedContent(tabItem);
                if (cachedContent != null) return cachedContent;
                cachedContent = new ContentControl
                {
                    DataContext = item,
                    ContentTemplate = GetTemplate(TabControl),
                    ContentTemplateSelector = GetTemplateSelector(TabControl)
                };

                cachedContent.SetBinding(ContentControl.ContentProperty, new Binding());
                SetInternalCachedContent(tabItem, cachedContent);

                return cachedContent;
            }
        }
    }
}