using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel vm;
        private bool isClosing;
        private bool forceClose;

        public MainWindow()
        {
            InitializeComponent();
            vm = DataContext as MainViewModel;

            CheckCleanupFile();
            vm.ShowChangelogIfUpdated();

            Mediator.Instance.Register(o =>
            {
                if (isClosing && (bool)o)
                {
                    forceClose = true;
                    Close();
                }
            }, ViewModelMessage.UnsavedChangesDialogClosed);
        }

        private void CheckCleanupFile()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\cleanup.txt"))
            {
                Process[] procList = Process.GetProcessesByName("slc_layouteditor");
                if (procList.Length > 1)
                {
                }

                int cycleCount = 0;
                while (Process.GetProcessesByName("slc_layouteditor").Length > 1)
                {
                    if (cycleCount == 3)
                    {
                        if (MessageBox.Show("Your old SLC Layout Editor instance needs to be closed in order to cleanup after updating!\n" +
                            "Shall I close them for you?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            foreach (Process proc in procList)
                            {
                                if (Process.GetCurrentProcess().Id != proc.Id)
                                {
                                    proc.Kill();
                                }
                            }
                        }
                        else
                        {
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    Thread.Sleep(1000);
                    cycleCount++;
                }

                string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\cleanup.txt");
                foreach (string path in lines)
                {
                    Util.SafeDeleteFile(path);
                }

                Util.SafeDeleteFile(AppDomain.CurrentDomain.BaseDirectory + "\\cleanup.txt");
            }
        }

        private void OpenWelcomeScreen_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowWelcomeScreen();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowSettings();
        }

        private void ReturnToEditor_Click(object sender, RoutedEventArgs e)
        {
            vm.ReturnToEditor();
        }

        private void SearchUpdates_Click(object sender, RoutedEventArgs e)
        {
            vm.UpdateManager.CheckForUpdates();
        }

        private void DownloadAndInstallUpdate_Click(object sender, RoutedEventArgs e)
        {
            vm.UpdateManager.DownloadUpdate();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose && vm.CheckUnsavedChanges(true))
            {
                isClosing = true;
                e.Cancel = true;
            }
        }

        private void Changelog_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowChangelog();
        }

        private void Roadmap_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://trello.com/b/vJMbqwXb/slc-layout-editor-roadmap");
        }
    }
}
