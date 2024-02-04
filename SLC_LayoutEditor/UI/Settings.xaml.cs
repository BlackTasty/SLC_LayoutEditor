using SLC_LayoutEditor.Controls.Notifications;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.UI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : DockPanel
    {
        private SettingsViewModel vm;
        private bool areSeasonalThemesEnabled;

        public Settings()
        {
            InitializeComponent();
            vm = DataContext as SettingsViewModel;
            ReloadSettings();
            areSeasonalThemesEnabled = App.Settings.EnableSeasonalThemes;
        }

        private void OpenEditFolder_Click(object sender, RoutedEventArgs e)
        {
            Util.OpenFolder(App.Settings.CabinLayoutsEditPath, false);
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Save(App.Settings.FilePath);
            Mediator.Instance.NotifyColleagues(ViewModelMessage.SettingsSaved);

            Notification.MakeTimedNotification("Settings saved", "Your preferences have been saved!",
                5000, FixedValues.ICON_CHECK_CIRCLE);
            /*if (areSeasonalThemesEnabled != App.Settings.EnableSeasonalThemes)
            {
                Util.RefreshTheme(Application.Current);
            }*/
        }

        private void Rollback_Click(object sender, RoutedEventArgs e)
        {
            Logger.Default.WriteLog("Resetting app config...");
            ReloadSettings();
        }

        private void ReloadSettings()
        {
            App.Settings = AppSettings.Load(new System.IO.FileInfo("settings.json"));
            vm.Refresh();
        }
    }
}
