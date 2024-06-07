using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for AircraftLayoutsViewControl.xaml
    /// </summary>
    public partial class AircraftLayoutsViewControl : Border, INotifyPropertyChanged
    {
        private const double TILE_MAX_WIDTH = 870;
        private const double TILE_MIN_WIDTH = 539;

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the "PropertyChanged" event for the given property name.
        /// </summary>
        /// <param name="propertyName">Can be left empty when called from inside the target property. The display name of the property which changed.</param>
        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Fires the "PropertyChanged" event for every given property name.
        /// </summary>
        /// <param name="propertyName">A list of display names for properties which changed.</param>
        protected void InvokePropertyChanged(params string[] propertyNames)
        {
            for (int i = 0; i < propertyNames.Length; i++)
            {
                InvokePropertyChanged(propertyNames[i]);
            }
        }
        #endregion

        #region SelectedAirframe
        public CabinLayoutSet SelectedAirframe
        {
            get { return (CabinLayoutSet)GetValue(SelectedAirframeProperty); }
            set { SetValue(SelectedAirframeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedAirframe.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAirframeProperty =
            DependencyProperty.Register("SelectedAirframe", typeof(CabinLayoutSet), typeof(AircraftLayoutsViewControl), new PropertyMetadata(null));
        #endregion

        public bool IsTemplatingMode
        {
            get { return (bool)GetValue(IsTemplatingModeProperty); }
            set { SetValue(IsTemplatingModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTemplatingMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTemplatingModeProperty =
            DependencyProperty.Register("IsTemplatingMode", typeof(bool), typeof(AircraftLayoutsViewControl), new PropertyMetadata(false));

        public ContextMenu GuideMenu
        {
            get { return (ContextMenu)GetValue(GuideMenuProperty); }
            set { SetValue(GuideMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GuideMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GuideMenuProperty =
            DependencyProperty.Register("GuideMenu", typeof(ContextMenu), typeof(AircraftLayoutsViewControl), new PropertyMetadata(null));

        public bool IsHeaderEnabled
        {
            get { return (bool)GetValue(IsHeaderEnabledProperty); }
            set { SetValue(IsHeaderEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHeaderEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHeaderEnabledProperty =
            DependencyProperty.Register("IsHeaderEnabled", typeof(bool), typeof(AircraftLayoutsViewControl), new PropertyMetadata(true));

        public CabinLayout SelectedCabinLayout
        {
            get { return (CabinLayout)GetValue(SelectedCabinLayoutProperty); }
            set { SetValue(SelectedCabinLayoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCabinLayout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCabinLayoutProperty =
            DependencyProperty.Register("SelectedCabinLayout", typeof(CabinLayout), typeof(AircraftLayoutsViewControl), new PropertyMetadata(null));

        public AircraftLayoutsViewControl()
        {
            InitializeComponent();
        }

        private void UpdateItemSize()
        {

        }

        private void CreateLayout_Click(object sender, RoutedEventArgs e)
        {
            IDialog dialog = Util.BeginCreateCabinLayout(false, SelectedAirframe);
            dialog.DialogClosing += CreateCabinLayout_DialogClosing;

            dialog.ShowDialog();
        }

        private void CreateTemplate_Click(object sender, RoutedEventArgs e)
        {
            IDialog dialog = Util.BeginCreateCabinLayout(true, SelectedAirframe);
            dialog.DialogClosing += CreateTemplate_DialogClosing;

            dialog.ShowDialog();
        }

        private void CreateTemplate_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is CreateTemplateDialog dialog)
            {
                dialog.DialogClosing -= CreateTemplate_DialogClosing;
            }

            if (e.DialogResult == DialogResultType.OK)
            {
                if (e.Data is string templateName)
                {
                    CabinLayout layout = new CabinLayout(templateName, SelectedAirframe.AircraftName, true);
                    layout.SaveLayout();
                    SelectedAirframe.RegisterLayout(layout);
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Load, layout);
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.ForceTemplatingToggleState, true);
                }
            }
            else
            {
                Logger.Default.WriteLog("Creating template aborted by user");
            }
        }

        private void CreateCabinLayout_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is CreateCabinLayoutDialog dialog)
            {
                dialog.DialogClosing -= CreateCabinLayout_DialogClosing;
            }

            if (e.DialogResult == DialogResultType.OK)
            {
                if (e.Data is AddDialogResult result && result.IsCreate)
                {
                    CabinLayout layout;
                    if (result.SelectedTemplatePath == null)
                    {
                        layout = new CabinLayout(result.Name, SelectedAirframe.AircraftName, false);
                        layout.SaveLayout();
                    }
                    else
                    {
                        string layoutPath = Path.Combine(App.Settings.CabinLayoutsEditPath, SelectedAirframe.AircraftName, result.Name + ".txt");
                        File.Copy(result.SelectedTemplatePath, layoutPath, true);
                        layout = new CabinLayout(new FileInfo(layoutPath));
                    }
                    SelectedAirframe.RegisterLayout(layout);
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Load, layout);
                    Mediator.Instance.NotifyColleagues(ViewModelMessage.ForceTemplatingToggleState, false);
                }
            }
            else
            {
                Logger.Default.WriteLog("Creating layout aborted by user");
            }
        }

        private void DeleteAircraft_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Delete aircraft",
                "Are you sure you want to delete your aircraft and all of its layouts and templates? This action cannot be undone!",
                DialogType.YesNo);

            dialog.DialogClosing += DeleteAircraft_DialogClosing;
            dialog.ShowDialog();
        }

        private void DeleteAircraft_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                SelectedAirframe.Delete();
            }
        }

        private void EditAircraftName_Click(object sender, RoutedEventArgs e)
        {
            /*IEnumerable<string> existingNames = App.;
            IDialog dialog = new EditNameDialog(string.Format("Edit {0} name", !layout.IsTemplate ? "cabin layout" : "template"),
            !layout.IsTemplate ? "Layout name" : "Template name", existingNames, layout.LayoutName);

            dialog.DialogClosing += EditLayoutName_DialogClosing;
            dialog.ShowDialog();*/
        }

        private void LayoutTile_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is CabinLayout cabinLayout)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Layout_Load, cabinLayout);
            }
        }

        private void ListBoxItem_Loaded(object sender, RoutedEventArgs e)
        {
            CabinLayoutTile layoutTile = Util.GetTemplatedControlFromListBoxItem<CabinLayoutTile>(sender, "layout_tile");

            if (layoutTile != null && ((ListBoxItem)sender).DataContext is CabinLayout cabinLayout)
            {
                layoutTile.CabinLayout = cabinLayout;
            }
        }

        private void Container_LayoutDetails_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                UpdateListPadding(e.NewSize.Height);
            }
        }

        private void Container_LayoutDetails_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateListPadding(container_details.Visibility == Visibility.Visible ? container_details.ActualHeight : 0);
        }

        private void UpdateListPadding(double bottomPadding)
        {
            list_layouts.Padding = new Thickness(30, 63, 0, bottomPadding);
        }
    }
}
