using System;
using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.UI
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : DockPanel
    {
        public event EventHandler<EventArgs> WelcomeConfirmed;

        public WelcomeScreen()
        {
            InitializeComponent();
        }

        protected virtual void OnWelcomeConfirmed(EventArgs e)
        {
            App.Settings.WelcomeScreenShown = true;
            App.SaveAppSettings();
            WelcomeConfirmed?.Invoke(this, e);
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            OnWelcomeConfirmed(EventArgs.Empty);
        }
    }
}
