namespace CustomControls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public partial class InitControl : UserControl
    {
        public InitControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (Parent is Panel AsPanel)
            {
                int ThisIndex = AsPanel.Children.IndexOf(this);
                if (ThisIndex > 0)
                {
                    if (AsPanel.Children[ThisIndex - 1] is FrameworkElement Sibling)
                    {
                        switch (Sibling)
                        {
                            case ToggleButton AsToggleButton:
                                if (AsToggleButton.IsThreeState)
                                {
                                    if (AsToggleButton.IsChecked.HasValue)
                                        OnUserInitialized();
                                    else
                                    {
                                        AsToggleButton.Checked += OnUserInitialized;
                                        AsToggleButton.Unchecked += OnUserInitialized;
                                    }
                                }

                                break;
                            case Selector AsSelector:
                                if (AsSelector.SelectedIndex >= 0)
                                    OnUserInitialized();
                                else
                                    AsSelector.SelectionChanged += OnUserInitialized;
                                break;
                            case TextBox AsTextBox:
                                if (AsTextBox.Text.Length > 0)
                                    OnUserInitialized();
                                else
                                    AsTextBox.TextChanged += OnUserInitialized;
                                break;
                        }
                    }
                }
            }
        }

        private void OnUserInitialized(object sender, SelectionChangedEventArgs args)
        {
            OnUserInitialized();
        }

        private void OnUserInitialized(object sender, RoutedEventArgs args)
        {
            if (sender is ToggleButton AsToggleButton)
                AsToggleButton.IsThreeState = false;

            OnUserInitialized();
        }

        private void TextChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            OnUserInitialized();
        }

        private void OnUserInitialized()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
