using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls.Cabin
{
    /// <summary>
    /// Interaction logic for CabinLayoutTile.xaml
    /// </summary>
    public partial class CabinLayoutTile : Border
    {
        private readonly CabinLayoutTileViewModel vm;

        public CabinLayout CabinLayout
        {
            get { return (CabinLayout)GetValue(CabinLayoutProperty); }
            set { SetValue(CabinLayoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CabinLayout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinLayoutProperty =
            DependencyProperty.Register("CabinLayout", typeof(CabinLayout), typeof(CabinLayoutTile), 
                new FrameworkPropertyMetadata(null, OnCabinLayoutChanged));

        private static void OnCabinLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CabinLayoutTile control)
            {
                CabinLayoutTileViewModel vm = (CabinLayoutTileViewModel)control.DataContext;
                vm.CabinLayout = control.CabinLayout;
                vm.LoadThumbnails();
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(CabinLayoutTile), new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CabinLayoutTile control)
            {
                CabinLayoutTileViewModel vm = (CabinLayoutTileViewModel)control.DataContext;
                vm.IsSelected = control.IsSelected;
            }
        }

        public CabinLayoutTile()
        {
            InitializeComponent();
            vm = (CabinLayoutTileViewModel)DataContext;

            Mediator.Instance.Register(o =>
            {
                if (o is LayoutTileRefreshData data && data.CabinLayout != null && data.CabinLayout.Guid == CabinLayout.Guid)
                {
                    vm.CabinLayout = data.CabinLayout;
                    if (!data.IsLoadingOnly)
                    {
                        vm.GenerateThumbnails();
                    }
                    else
                    {
                        vm.LoadThumbnails();
                    }
                }
            }, ViewModelMessage.Layout_Tile_RefreshData);
        }

        private void PreviousThumbnail_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            vm.ThumbnailIndex--;
        }

        private void NextThumbnail_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            vm.ThumbnailIndex++;
        }

        private void GenerateThumbnail_Click(object sender, RoutedEventArgs e)
        {
            vm.GenerateThumbnails();
        }

        private void DeleteLayoutTemplate_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ConfirmationDialog dialog = new ConfirmationDialog(!CabinLayout.IsTemplate ? "Delete cabin layout" : "Delete template",
                "Are you sure you want to delete your " + (!CabinLayout.IsTemplate ? "cabin layout" : "template") + "? This action cannot be undone!",
                DialogType.YesNo);

            dialog.DialogClosing += DeleteLayout_DialogClosing;
            dialog.ShowDialog();
        }

        private void DeleteLayout_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                CabinLayout.Delete();
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
