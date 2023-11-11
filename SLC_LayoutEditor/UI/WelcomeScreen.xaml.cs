using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void WelcomeScreen_CopyDone(object sender, EventArgs e)
        {
            OnWelcomeConfirmed(EventArgs.Empty);
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

        private async void CopyLayouts_Click(object sender, RoutedEventArgs e)
        {
            await (DataContext as WelcomeScreenViewModel).RunCopy();

            OnWelcomeConfirmed(EventArgs.Empty);
        }

        private void SelectSLCPath_Click(object sender, RoutedEventArgs e)
        {
            string path = Util.SelectFolder("Select SLC cabin layouts folder",
                                                        App.Settings.CabinLayoutsReadoutPath,
                                                        false);
            if (path != null)
            {
                App.Settings.CabinLayoutsReadoutPath = path;
            }
        }

        private void SelectCopyTargetPath_Click(object sender, RoutedEventArgs e)
        {
            string path = Util.SelectFolder("Select a folder to copy all layouts to",
                                                        App.Settings.CabinLayoutsEditPath,
                                                        true);
            if (path != null)
            {
                App.Settings.CabinLayoutsEditPath = path;
            }
        }

        private void OpenReadoutFolder_Click(object sender, RoutedEventArgs e)
        {
            Util.OpenFolder(App.Settings.CabinLayoutsReadoutPath, false);
        }

        private void OpenEditFolder_Click(object sender, RoutedEventArgs e)
        {
            Util.OpenFolder(App.Settings.CabinLayoutsEditPath, false);
        }
    }
}
