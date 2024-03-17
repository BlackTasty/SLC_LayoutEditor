using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for Updater.xaml
    /// </summary>
    public partial class Updater : StackPanel
    {
        public event EventHandler<EventArgs> InstallUpdateClicked;

        private readonly UpdaterViewModel vm;

        public Updater()
        {
            InitializeComponent();
            vm = DataContext as UpdaterViewModel;
        }

        private void SearchUpdates_Click(object sender, RoutedEventArgs e)
        {
            ProceedThroughUpdate();
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.IsPressed = true;
            e.Handled = true;
        }

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (vm.IsPressed)
            {
                ProceedThroughUpdate();
                vm.IsPressed = false;
                e.Handled = true;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            vm.IsPressed = false;
            vm.IsMouseOver = false;
        }

        private void ProceedThroughUpdate()
        {
            if (vm.UpdateStatus == Core.Patcher.UpdateStatus.UPDATES_FOUND)
            {
                vm.Patcher.DownloadUpdate();
            }
            else if (vm.UpdateStatus == Core.Patcher.UpdateStatus.READY)
            {
                App.Patcher.RestartAfterClose = true;
                Application.Current.MainWindow.Close();
            }
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ForceRefreshProperties();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            vm.IsMouseOver = true;
        }

        protected virtual void OnInstallUpdateClicked(EventArgs e)
        {
            InstallUpdateClicked?.Invoke(this, e);
        }
    }
}
