namespace CustomControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Interaction logic for CustomPopupButton.xaml.
    /// </summary>
    public partial class CustomPopupButton : UserControl
    {
        #region Init
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPopupButton"/> class.
        /// </summary>
        public CustomPopupButton()
        {
            InitializeComponent();

            Application CurrentApp = Application.Current;
            CurrentApp.Deactivated += OnDeactivated;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            ControlList.Add(Control);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            ControlList.Remove(Control);
        }

        private static List<CustomPopupButton> ControlList = new();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether the control is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// The IsExpanded dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(CustomPopupButton), new UIPropertyMetadata(false, OnIsExpandedChanged));

        private static void OnIsExpandedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is bool NewIsExpanded)
                Control.OnIsExpandedChanged(NewIsExpanded);
        }

        /// <summary>
        /// Called when the IsExpanded property has changed.
        /// </summary>
        /// <param name="newIsExpanded">The new value.</param>
        public void OnIsExpandedChanged(bool newIsExpanded)
        {
            if (newIsExpanded)
            {
                foreach (CustomPopupButton Control in ControlList)
                    if (Control != this)
                        Control.IsExpanded = false;
            }
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// The Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(CustomPopupButton), new UIPropertyMetadata(string.Empty, OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is string NewHeader)
                Control.OnHeaderChanged(NewHeader);
        }

        /// <summary>
        /// Called when the Header property has changed.
        /// </summary>
        /// <param name="newHeader">The new value.</param>
        public void OnHeaderChanged(string newHeader)
        {
            NotifyPropertyChanged(nameof(HeaderMargin));
        }

        /// <summary>
        /// Gets or sets the popup content.
        /// </summary>
        public new object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// The Content dependency property.
        /// </summary>
        public static readonly new DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(CustomPopupButton), new UIPropertyMetadata(null, OnContentChanged));

        private static void OnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            Control.OnContentChanged(e.NewValue);
        }

        /// <summary>
        /// Called when the Content property has changed.
        /// </summary>
        /// <param name="newContent">The new value.</param>
        public void OnContentChanged(object newContent)
        {
        }

        /// <summary>
        /// Gets or sets the popup content template.
        /// </summary>
        public new DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        /// <summary>
        /// The ContentTemplate dependency property.
        /// </summary>
        public static readonly new DependencyProperty ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(CustomPopupButton), new UIPropertyMetadata(new DataTemplate(), OnContentTemplateChanged));

        private static void OnContentTemplateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is DataTemplate NewContentTemplate)
                Control.OnContentTemplateChanged(NewContentTemplate);
        }

        /// <summary>
        /// Called when the ContentTemplate property has changed.
        /// </summary>
        /// <param name="newContentTemplate">The new value.</param>
        public void OnContentTemplateChanged(DataTemplate newContentTemplate)
        {
        }

        /// <summary>
        /// Gets or sets the popup width.
        /// </summary>
        public double PopupWidth
        {
            get { return (double)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        /// <summary>
        /// The PopupWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupWidthProperty = DependencyProperty.Register("PopupWidth", typeof(double), typeof(CustomPopupButton), new UIPropertyMetadata(double.NaN, OnPopupWidthChanged));

        private static void OnPopupWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is double NewPopupWidth)
                Control.OnPopupWidthChanged(NewPopupWidth);
        }

        /// <summary>
        /// Called when the PopupWidth property has changed.
        /// </summary>
        /// <param name="newPopupWidth">The new value.</param>
        public void OnPopupWidthChanged(double newPopupWidth)
        {
        }

        /// <summary>
        /// Gets or sets the popup height.
        /// </summary>
        public double PopupHeight
        {
            get { return (double)GetValue(PopupHeightProperty); }
            set { SetValue(PopupHeightProperty, value); }
        }

        /// <summary>
        /// The PopupHeight dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupHeightProperty = DependencyProperty.Register("PopupHeight", typeof(double), typeof(CustomPopupButton), new UIPropertyMetadata(double.NaN, OnPopupHeightChanged));

        private static void OnPopupHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is double NewPopupHeight)
                Control.OnPopupHeightChanged(NewPopupHeight);
        }

        /// <summary>
        /// Called when the PopupHeight property has changed.
        /// </summary>
        /// <param name="newPopupHeight">The new value.</param>
        public void OnPopupHeightChanged(double newPopupHeight)
        {
        }

        /// <summary>
        /// Gets or sets the horizontal offset.
        /// </summary>
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        /// <summary>
        /// The HorizontalOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(CustomPopupButton), new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));

        private static void OnHorizontalOffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is double NewHorizontalOffset)
                Control.OnHorizontalOffsetChanged(NewHorizontalOffset);
        }

        /// <summary>
        /// Called when the HorizontalOffset property has changed.
        /// </summary>
        /// <param name="newHorizontalOffset">The new value.</param>
        public void OnHorizontalOffsetChanged(double newHorizontalOffset)
        {
        }

        /// <summary>
        /// Gets or sets the vertical offset.
        /// </summary>
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        /// <summary>
        /// The VerticalOffset dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(CustomPopupButton), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is double NewVerticalOffset)
                Control.OnVerticalOffsetChanged(NewVerticalOffset);
        }

        /// <summary>
        /// Called when the VerticalOffset property has changed.
        /// </summary>
        /// <param name="newVerticalOffset">The new value.</param>
        public void OnVerticalOffsetChanged(double newVerticalOffset)
        {
        }

        /// <summary>
        /// Gets or sets the button alignment.
        /// </summary>
        public HorizontalAlignment ButtonAlignment
        {
            get { return (HorizontalAlignment)GetValue(ButtonAlignmentProperty); }
            set { SetValue(ButtonAlignmentProperty, value); }
        }

        /// <summary>
        /// The ButtonAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonAlignmentProperty = DependencyProperty.Register("ButtonAlignment", typeof(HorizontalAlignment), typeof(CustomPopupButton), new UIPropertyMetadata(HorizontalAlignment.Left, OnButtonAlignmentChanged));

        private static void OnButtonAlignmentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomPopupButton Control = (CustomPopupButton)sender;
            if (e.NewValue is HorizontalAlignment NewButtonAlignment)
                Control.OnButtonAlignmentChanged(NewButtonAlignment);
        }

        /// <summary>
        /// Called when the ButtonAlignment property has changed.
        /// </summary>
        /// <param name="newButtonAlignment">The new value.</param>
        public void OnButtonAlignmentChanged(HorizontalAlignment newButtonAlignment)
        {
            switch (newButtonAlignment)
            {
                default:
                case HorizontalAlignment.Left:
                    HeaderButton.SetValue(DockPanel.DockProperty, Dock.Left);
                    HeaderText.SetValue(DockPanel.DockProperty, Dock.Left);
                    break;
                case HorizontalAlignment.Right:
                    HeaderButton.SetValue(DockPanel.DockProperty, Dock.Right);
                    HeaderText.SetValue(DockPanel.DockProperty, Dock.Right);
                    break;
            }

            NotifyPropertyChanged(nameof(HeaderMargin));
        }

        /// <summary>
        /// Gets the header margin.
        /// </summary>
        public Thickness HeaderMargin
        {
            get
            {
                if (Header.Length == 0)
                    return default(Thickness);

                switch (ButtonAlignment)
                {
                    default:
                    case HorizontalAlignment.Left:
                        return new Thickness(5, 0, 0, 0);
                    case HorizontalAlignment.Right:
                        return new Thickness(0, 0, 5, 0);
                }
            }
        }
        #endregion

        #region Events
#nullable disable annotations
        private void OnDeactivated(object sender, EventArgs e)
#nullable restore annotations
        {
            if (IsExpanded)
                IsExpanded = false;
        }

        public CustomPopupPlacement[] OnCustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            double HorizontalOffset;

            switch (ButtonAlignment)
            {
                default:
                case HorizontalAlignment.Left:
                    HorizontalOffset = 0;
                    break;
                case HorizontalAlignment.Right:
                    HorizontalOffset = targetSize.Width - popupSize.Width;
                    break;
            }

            CustomPopupPlacement Placement = new(new Point(HorizontalOffset, targetSize.Height), PopupPrimaryAxis.None);
            return new CustomPopupPlacement[] { Placement };
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        /// <summary>
        /// Implements the PropertyChanged event.
        /// </summary>
#nullable disable annotations
        public event PropertyChangedEventHandler PropertyChanged;
#nullable restore annotations

        internal void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void NotifyThisPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
