using SLC_LayoutEditor.ViewModel;
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

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for Updater.xaml
    /// </summary>
    public partial class Updater : StackPanel
    {
        private readonly MainViewModel vm;
        private bool isMouseDown;

        public Updater()
        {
            InitializeComponent();
            vm = DataContext as MainViewModel;
        }

        private void SearchUpdates_Click(object sender, RoutedEventArgs e)
        {
            ProceedThroughUpdate();
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
        }

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                ProceedThroughUpdate();
                isMouseDown = false;
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private void ProceedThroughUpdate()
        {
            if (vm.UpdateStatus == Core.Patcher.UpdateStatus.UPDATES_FOUND)
            {
                vm.UpdateManager.DownloadUpdate();
            }
        }
    }
}
