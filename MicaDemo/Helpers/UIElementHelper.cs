using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace MicaDemo.Helpers
{
    public static class UIElementHelper
    {
        #region ContextFlyout

        /// <summary>
        /// Gets the flyout associated with this element.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The flyout associated with this element, if any; otherwise, <see langword="null"/>. The default is <see langword="null"/>.</returns>
        public static FlyoutBase GetContextFlyout(UIElement element)
        {
            return (FlyoutBase)element.GetValue(ContextFlyoutProperty);
        }

        /// <summary>
        /// Sets the flyout associated with this element.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The flyout associated with this element.</param>
        public static void SetContextFlyout(UIElement element, FlyoutBase value)
        {
            element.SetValue(ContextFlyoutProperty, value);
        }

        /// <summary>
        /// Identifies the ContextFlyout dependency property.
        /// </summary>
        public static readonly DependencyProperty ContextFlyoutProperty =
            DependencyProperty.RegisterAttached(
                "ContextFlyout",
                typeof(FlyoutBase),
                typeof(UIElementHelper),
                new PropertyMetadata(null, OnContextFlyoutChanged));

        private static void OnContextFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = (UIElement)d;
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "ContextFlyout"))
            {
                element.ContextFlyout = GetContextFlyout(element);
            }
            else if (element is FrameworkElement frameworkElement)
            {
                FlyoutBase.SetAttachedFlyout(frameworkElement, e.NewValue as FlyoutBase);

                element.KeyDown -= OnKeyDown;
                element.Holding -= OnHolding;
                element.RightTapped -= OnRightTapped;

                if (element != null)
                {
                    element.KeyDown += OnKeyDown;
                    element.Holding += OnHolding;
                    element.RightTapped += OnRightTapped;
                }
            }
        }

        private static void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e?.Handled == true) { return; }
            if (e.Key == VirtualKey.Menu)
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
                if (e != null) { e.Handled = true; }
            }
        }

        private static void OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (e?.Handled == true || !(sender is FrameworkElement element)) { return; }
            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);
            if (flyout is MenuFlyout menu)
            {
                menu.ShowAt(element, e.GetPosition(element));
            }
            else
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }
            if (e != null) { e.Handled = true; }
        }

        private static void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e?.Handled == true || !(sender is FrameworkElement element)) { return; }
            FlyoutBase flyout = FlyoutBase.GetAttachedFlyout(element);
            if (flyout is MenuFlyout menu)
            {
                menu.ShowAt(element, e.GetPosition(element));
            }
            else
            {
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }
            if (e != null) { e.Handled = true; }
        }

        #endregion

        #region Icon

        /// <summary>
        /// Gets the graphic content of the menu flyout item.
        /// </summary>
        /// <param name="control">The element from which to read the property value.</param>
        /// <returns>The graphic content of the menu flyout item.</returns>
        public static IconElement GetIcon(MenuFlyoutItemBase control)
        {
            return (IconElement)control.GetValue(IconProperty);
        }

        /// <summary>
        /// Sets the graphic content of the menu flyout item.
        /// </summary>
        /// <param name="control">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetIcon(MenuFlyoutItemBase control, IconElement value)
        {
            control.SetValue(IconProperty, value);
        }

        /// <summary>
        /// Identifies the Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(IconElement),
                typeof(UIElementHelper),
                new PropertyMetadata(null, OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuFlyoutItem item)
            {
                if (e.NewValue is IconElement IconElement && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutItem", "Icon"))
                {
                    item.Icon = IconElement;
                }
            }
            else if (d is MenuFlyoutSubItem subitem)
            {
                if (e.NewValue is IconElement IconElement && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutSubItem", "Icon"))
                {
                    subitem.Icon = IconElement;
                }
            }
            else if (d is ToggleMenuFlyoutItem toggle)
            {
                if (e.NewValue is IconElement IconElement && ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ToggleMenuFlyoutItem", "Icon"))
                {
                    toggle.Icon = IconElement;
                }
            }
        }

        #endregion

        #region BackgroundTransition

        /// <summary>
        /// Gets an instance of BrushTransition to automatically animate changes to the Background property.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>An instance of BrushTransition to automatically animate changes to the Background; otherwise, <see langword="null"/>. The default is <see langword="null"/>.</returns>
        public static bool GetBackgroundTransition(UIElement element)
        {
            return (bool)element.GetValue(BackgroundTransitionProperty);
        }

        /// <summary>
        /// Sets an instance of BrushTransition to automatically animate changes to the Background property.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The instance of BrushTransition to automatically animate changes to the Background property.</param>
        public static void SetBackgroundTransition(UIElement element, bool value)
        {
            element.SetValue(BackgroundTransitionProperty, value);
        }

        /// <summary>
        /// Identifies the BackgroundTransition dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.RegisterAttached(
                "BackgroundTransition",
                typeof(bool),
                typeof(UIElementHelper),
                new PropertyMetadata(false, OnBackgroundTransitionChanged));

        private static void OnBackgroundTransitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.BrushTransition"))
            {
                switch (d)
                {
                    case Panel panel:
                        panel.BackgroundTransition = e.NewValue is true ? new BrushTransition() : null;
                        break;
                    case Border border:
                        border.BackgroundTransition = e.NewValue is true ? new BrushTransition() : null;
                        break;
                    case ContentPresenter contentPresenter:
                        contentPresenter.BackgroundTransition = e.NewValue is true ? new BrushTransition() : null;
                        break;
                }
            }
        }

        #endregion
    }
}
