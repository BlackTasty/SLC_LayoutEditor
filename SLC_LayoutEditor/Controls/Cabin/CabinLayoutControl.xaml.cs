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
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls.Cabin
{
    /// <summary>
    /// Interaction logic for CabinDeckControl.xaml
    /// </summary>
    public partial class CabinLayoutControl : DockPanel, INotifyPropertyChanged, IDisposable
    {
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

        public event EventHandler<EventArgs> LayoutRegenerated;
        public event EventHandler<EventArgs> LayoutReloaded;
        public event EventHandler<EventArgs> LayoutLoading;
        public event EventHandler<SelectedSlotsChangedEventArgs> SelectedSlotsChanged;
        public event EventHandler<ChangedEventArgs> Changed;
        public event EventHandler<EventArgs> TemplatingModeToggled;
        public event EventHandler<TemplateCreatedEventArgs> TemplateCreated;

        public event EventHandler<CabinDeckChangedEventArgs> CabinDeckChanged;

        private CabinDeck currentRemoveTarget;

        #region CabinLayout property
        public CabinLayout CabinLayout
        {
            get { return (CabinLayout)GetValue(CabinLayoutProperty); }
            set { SetValue(CabinLayoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CabinLayout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinLayoutProperty =
            DependencyProperty.Register("CabinLayout", typeof(CabinLayout), typeof(CabinLayoutControl), new PropertyMetadata(null,
                OnCabinLayoutChanged));

        private static void OnCabinLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is CabinLayoutControl control)
            {
                CabinLayout oldLayout = e.OldValue as CabinLayout;
                CabinLayout newLayout = e.NewValue as CabinLayout;
                Logger.Default.WriteLog("Layout has changed for UI. Old data: {0}; New data: {1}", 
                    GetCabinLayoutValueForLog(oldLayout), GetCabinLayoutValueForLog(newLayout));
                control.RefreshCabinLayout(true);
            }
        }

        private static string GetCabinLayoutValueForLog(CabinLayout cabinLayout)
        {
            return cabinLayout != null ?
                string.Format("{0} (type: {1})", cabinLayout.LayoutName, cabinLayout.IsTemplate ? "template" : "layout") :
                "<UNSET>";
        }
        #endregion

        public ContextMenu GuideMenu
        {
            get { return (ContextMenu)GetValue(GuideMenuProperty); }
            set { SetValue(GuideMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GuideMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GuideMenuProperty =
            DependencyProperty.Register("GuideMenu", typeof(ContextMenu), typeof(CabinLayoutControl), new PropertyMetadata(null));

        public string LayoutOverviewTitle => CabinLayout != null ?
            string.Format("{0} {1}", CabinLayout.LayoutName, HasUnsavedChanges ? "*" : "") :
            "No layout selected";

        public bool HasUnsavedChanges => Util.HasLayoutChanged(CabinLayout);

        public bool IsHorizontalScrollBarVisible => deck_scroll.ComputedHorizontalScrollBarVisibility == Visibility.Visible;

        public bool IsVerticalScrollBarVisible => deck_scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible;

        #region SelectedMultiSlotTypeIndex properties
        public int SelectedMultiSlotTypeIndex
        {
            get { return (int)GetValue(SelectedMultiSlotTypeIndexProperty); }
            set { SetValue(SelectedMultiSlotTypeIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedMultiSlotTypeIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedMultiSlotTypeIndexProperty =
            DependencyProperty.Register("SelectedMultiSlotTypeIndex", typeof(int), typeof(CabinLayoutControl), new PropertyMetadata(-1));
        #endregion

        public bool IsTemplatingMode
        {
            get { return (bool)GetValue(IsTemplatingModeProperty); }
            set { SetValue(IsTemplatingModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTemplatingMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTemplatingModeProperty =
            DependencyProperty.Register("IsTemplatingMode", typeof(bool), typeof(CabinLayoutControl), new PropertyMetadata(false, OnIsTemplatingModeChanged));

        public bool IsSidebarOpen
        {
            get { return (bool)GetValue(IsSidebarOpenProperty); }
            set { SetValue(IsSidebarOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSidebarOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSidebarOpenProperty =
            DependencyProperty.Register("IsSidebarOpen", typeof(bool), typeof(CabinLayoutControl), new PropertyMetadata(true));

        private static void OnIsTemplatingModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is CabinLayoutControl control)
            {
                control.OnTemplatingModeToggled(EventArgs.Empty);
            }
        }

        public CabinLayoutControl()
        {
            InitializeComponent();

            Mediator.Instance.Register(o =>
            {
                InvokePropertyChanged(nameof(LayoutOverviewTitle));
            }, ViewModelMessage.LayoutNameChanged);

            Mediator.Instance.Register(o =>
            {
                IsTemplatingMode = (bool)o;
            }, ViewModelMessage.ForceTemplatingToggleState);

            Mediator.Instance.Register(o =>
            {
                if (o is CabinLayout cabinLayout)
                {
                    MakeTemplate(cabinLayout);
                }
            }, ViewModelMessage.Keybind_MakeTemplate);

            Mediator.Instance.Register(o =>
            {
                StartReloadLayout();
            }, ViewModelMessage.Keybind_ReloadLayout);

            Mediator.Instance.Register(o =>
            {
                foreach (CabinDeckControl deckLayoutControl in container_decks.Children.OfType<CabinDeckControl>())
                {
                    deckLayoutControl.SelectAllSlots(true);
                }
            }, ViewModelMessage.SelectAll_Layout);

            Mediator.Instance.Register(o =>
            {
                if (o is int floor)
                {
                    container_decks.Children.OfType<CabinDeckControl>()
                        .FirstOrDefault(x => x.CabinDeck.Floor == floor)
                        ?.SelectAllSlots(true);
                }
            }, ViewModelMessage.SelectAll_Deck);

            Mediator.Instance.Register(o =>
            {
                if (o is IEnumerable<CabinSlot> cabinSlots)
                {
                    foreach (CabinDeckControl deckControl in container_decks?.Children.OfType<CabinDeckControl>())
                    {
                        IEnumerable<CabinSlot> selectedOnDeck = cabinSlots.Where(x => deckControl.CabinDeck.ContainsCabinSlot(x));
                        deckControl.DeselectSlots(selectedOnDeck);
                    }
                }
            }, ViewModelMessage.Deselect_Slots);
        }

        public void GenerateThumbnailsForLayout(bool overwrite = false)
        {
            if (CabinLayout != null)
            {
                Logger.Default.WriteLog("Generating thumbnail for {0} \"{1}\"...", CabinLayout.IsTemplate ? "template" : "layout", CabinLayout.LayoutName);
                Directory.CreateDirectory(CabinLayout.ThumbnailDirectory);

                foreach (CabinDeckControl deckLayoutControl in container_decks.Children.OfType<CabinDeckControl>())
                {
                    deckLayoutControl.GenerateThumbnailForDeck(CabinLayout.ThumbnailDirectory, overwrite);
                }

                CabinLayout.CheckThumbnailStore();
            }
        }

        public void RenderAdorners()
        {
            foreach (CabinDeckControl deckLayoutControl in container_decks.Children.OfType<CabinDeckControl>())
            {
                deckLayoutControl.RenderAdorners();
            }
        }

        public UIElement GetDeckIssueElement()
        {
            return container_decks.Children.OfType<CabinDeckControl>().FirstOrDefault().cards_deckIssues;
        }

        public void Dispose()
        {
            if (CabinLayout != null)
            {
                CabinLayout.CabinSlotsChanged -= CabinLayout_CabinSlotsChanged;
                CabinLayout.CabinDeckCountChanged -= CabinLayout_CabinDeckCountChanged;

                foreach (CabinDeckControl deckControl in container_decks?.Children.OfType<CabinDeckControl>())
                {
                    deckControl.LayoutRegenerated -= CabinDeckControl_LayoutRegenerated;
                    deckControl.RemoveDeckClicked -= CabinDeckControl_RemoveDeckClicked;

                    deckControl.RenderSizeChanged -= CabinDeckControl_RenderSizeChanged;
                }
            }
        }

        private void RefreshCabinLayout(bool hookCabinLayoutEvents)
        {
            OnLayoutLoading(EventArgs.Empty);

            //Unhook events before clearing container for deck layout controls
            foreach (CabinDeckControl cabinDeckControl in container_decks.Children.OfType<CabinDeckControl>())
            {
                cabinDeckControl.LayoutRegenerated -= CabinDeckControl_LayoutRegenerated;
                cabinDeckControl.RemoveDeckClicked -= CabinDeckControl_RemoveDeckClicked;
                //cabinDeckControl.LayoutLoading -= CabinDeckControl_LayoutLoading;

                cabinDeckControl.RenderSizeChanged -= CabinDeckControl_RenderSizeChanged;
                cabinDeckControl.DeckRendered -= CabinDeckControl_DeckRendered;

                cabinDeckControl.Dispose();
            }
            container_decks.Children.Clear();

            if (CabinLayout != null)
            {
                Logger.Default.WriteLog("Refreshing view for {0} \"{1}\"...", CabinLayout.IsTemplate ? "template" : "layout", CabinLayout.LayoutName);
                if (hookCabinLayoutEvents)
                {
                    CabinLayout.CabinSlotsChanged += CabinLayout_CabinSlotsChanged;
                    CabinLayout.CabinDeckCountChanged += CabinLayout_CabinDeckCountChanged;
                }

                foreach (CabinDeck cabinDeck in CabinLayout.CabinDecks)
                {
                    Logger.Default.WriteLog("Rendering cabin deck floor {0}...", cabinDeck.Floor);
                    AddCabinDeckToUI(cabinDeck);
                }
            }

            OnLayoutRegenerated(EventArgs.Empty);
            CabinLayout?.DeepRefreshProblemChecks();
            RefreshState();
            RefreshScrollBarVisibleFlags();
        }

        private void AddCabinDeckToUI(CabinDeck cabinDeck)
        {
            CabinDeckControl cabinDeckControl = new CabinDeckControl()
            {
                CabinDeck = cabinDeck,
                GuideMenu = GuideMenu
            };

            cabinDeckControl.SelectedSlotsChanged += CabinDeckControl_SelectedSlotsChanged;

            cabinDeckControl.LayoutRegenerated += CabinDeckControl_LayoutRegenerated;
            cabinDeckControl.RemoveDeckClicked += CabinDeckControl_RemoveDeckClicked;
            //cabinDeckControl.LayoutLoading += CabinDeckControl_LayoutLoading;

            cabinDeckControl.RenderSizeChanged += CabinDeckControl_RenderSizeChanged;
            cabinDeckControl.DeckRendered += CabinDeckControl_DeckRendered;
            container_decks.Children.Add(cabinDeckControl);
        }

        public void DeselectSlots()
        {
            foreach (CabinDeckControl cabinDeckControl in container_decks.Children.OfType<CabinDeckControl>())
            {
                cabinDeckControl.DeselectSlots();
            }
        }

        private void CabinDeckControl_SelectedSlotsChanged(object sender, SelectedSlotsChangedEventArgs e)
        {
            OnSelectedSlotsChanged(e);
        }

        private void CabinDeckControl_DeckRendered(object sender, EventArgs e)
        {
            Directory.CreateDirectory(CabinLayout.ThumbnailDirectory);
            if (sender is CabinDeckControl deckLayoutControl)
            {
                deckLayoutControl.GenerateThumbnailForDeck(CabinLayout.ThumbnailDirectory, true);
            }
        }

        private void CabinLayout_CabinDeckCountChanged(object sender, CabinDeckChangedEventArgs e)
        {
            if (e.IsRemoving)
            {
                if (container_decks.Children.OfType<CabinDeckControl>()
                        .FirstOrDefault(x => x.CabinDeck.Floor == e.TrueValue.Floor) is CabinDeckControl targetControl)
                {
                    targetControl.LayoutRegenerated -= CabinDeckControl_LayoutRegenerated;
                    targetControl.RemoveDeckClicked -= CabinDeckControl_RemoveDeckClicked;
                    targetControl.RenderSizeChanged -= CabinDeckControl_RenderSizeChanged;
                    targetControl.DeckRendered -= CabinDeckControl_DeckRendered;
                    container_decks.Children.Remove(targetControl);
                }
            }
            else
            {
                e.TrueValue.ThumbnailDirectory = CabinLayout.ThumbnailDirectory;

                AddCabinDeckToUI(e.TrueValue);
            }

            RefreshState();
        }

        private void CabinDeckControl_RenderSizeChanged(object sender, EventArgs e)
        {
            OnChanged(new ChangedEventArgs(Util.HasLayoutChanged(CabinLayout)));
            RefreshState();
        }

        private void CabinLayout_CabinSlotsChanged(object sender, EventArgs e)
        {
            RefreshState();
        }

        private void CabinDeckControl_LayoutRegenerated(object sender, EventArgs e)
        {
            OnChanged(new ChangedEventArgs(Util.HasLayoutChanged(CabinLayout)));
            RefreshState();
        }

        private void CabinDeckControl_RemoveDeckClicked(object sender, RemoveCabinDeckEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog("Confirm deletion",
                "Are you sure you want to delete this deck? This action can be undone by pressing Ctrl+Z or using the \"Undo\" button in the title bar.", DialogType.YesNo);
            currentRemoveTarget = e.Target;

            dialog.DialogClosing += ConfirmRemoveDeck_DialogClosing;

            dialog.ShowDialog();
        }

        private void ConfirmRemoveDeck_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                OnCabinDeckChanged(new CabinDeckChangedEventArgs(currentRemoveTarget, true));
                CabinLayout.RemoveCabinDeck(currentRemoveTarget);
                CabinLayout.RefreshData();
            }

            currentRemoveTarget = null;
        }

        protected virtual void OnCabinDeckChanged(CabinDeckChangedEventArgs e)
        {
            CabinDeckChanged?.Invoke(this, e);
        }

        /*private void CabinDeckControl_LayoutLoading(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }*/

        private void AddCabinDeck_Click(object sender, RoutedEventArgs e)
        {
            if (CabinLayout.CabinDecks.Count > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Match existing layout?",
                    "Should the new deck have matching rows and columns?", DialogType.YesNoCancel);

                dialog.DialogClosing += CreateDeck_DialogClosing;
                dialog.ShowDialog();
            }
            else
            {
                ShowDeckSizeDialog(7, 15);
            }
        }

        private void ShowDeckSizeDialog(int columns, int rows)
        {
            SpecifyDeckSizeDialog dialog = new SpecifyDeckSizeDialog(rows, columns);

            dialog.DialogClosing += SpecifyDeckSize_DialogClosing;
            dialog.ShowDialog();
        }

        private void CreateDeck_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            CabinDeck lastDeck = CabinLayout.CabinDecks.LastOrDefault();
            if (e.DialogResult == DialogResultType.No)
            {
                ShowDeckSizeDialog(lastDeck.Columns, lastDeck.Rows);
            }
            else if (e.DialogResult == DialogResultType.Yes)
            {
                CreateCabinDeck(lastDeck.Rows + 1, lastDeck.Columns + 1);
            }
        }

        private void SpecifyDeckSize_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (sender is IDialog dialog)
            {
                dialog.DialogClosing -= SpecifyDeckSize_DialogClosing;
            }

            if (e.DialogResult == DialogResultType.OK && e.Data is int[] deckData)
            {
                CreateCabinDeck(deckData[0], deckData[1]);
            }
        }

        private void CreateCabinDeck(int rows, int columns)
        {
            CabinDeck createdDeck = CabinLayout.AddCabinDeck(new CabinDeck(CabinLayout.CabinDecks.Count + 1, rows, columns));

            CabinLayout.RefreshData();
            OnCabinDeckChanged(new CabinDeckChangedEventArgs(createdDeck, false));
        }

        private void ReloadLayout_Click(object sender, RoutedEventArgs e)
        {
            StartReloadLayout();
        }

        private void StartReloadLayout()
        {
            if (Util.HasLayoutChanged(CabinLayout))
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Reload cabin layout",
                "Do you really want to reload this layout? Any unsaved changes are lost!", DialogType.YesNo);

                dialog.DialogClosing += ReloadDeck_DialogClosing;

                dialog.ShowDialog();
            }
            else
            {
                ReloadLayout();
            }
        }

        private void ReloadDeck_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                ReloadLayout();
            }
        }

        private void ReloadLayout()
        {
            CabinLayout.LoadCabinLayoutFromFile(true);
            RefreshCabinLayout(false);
            OnLayoutReloaded(EventArgs.Empty);
        }

        public void RefreshState(bool refreshProblemChecks = true)
        {
            InvokePropertyChanged(nameof(LayoutOverviewTitle));
            InvokePropertyChanged(nameof(HasUnsavedChanges));
            if (refreshProblemChecks)
            {
                CabinLayout?.DeepRefreshProblemChecks();
            }
        }

        public void RedrawDirtySlots()
        {
            foreach (CabinDeckControl cabinDeckControl in container_decks.Children.OfType<CabinDeckControl>())
            {
                cabinDeckControl.RedrawDirtySlots();
            }
        }

        private void MakeTemplate_Click(object sender, RoutedEventArgs e)
        {
            MakeTemplate(CabinLayout);
        }

        private void MakeTemplate(CabinLayout cabinLayout)
        {
            string templatesPath = !IsTemplatingMode ? App.GetTemplatePath(cabinLayout.LayoutFile.Directory.Name) :
                cabinLayout.LayoutFile.DirectoryName;
            IEnumerable<string> existingTemplates = Directory.Exists(templatesPath) ?
                new DirectoryInfo(templatesPath).EnumerateFiles("*.txt").Select(x => x.Name.Replace(".txt", "")) :
                new List<string>();

            MakeTemplateDialog dialog = new MakeTemplateDialog(existingTemplates, cabinLayout.LayoutName + " - Template", cabinLayout);
            dialog.DialogClosing += MakeTemplate_DialogClosing;

            dialog.ShowDialog();
        }

        private void MakeTemplate_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.OK && e.Data is MakeTemplateDialogViewModel data)
            {
                string layoutsPath = !IsTemplatingMode ? App.GetTemplatePath(CabinLayout.LayoutFile.Directory.Name) :
                    CabinLayout.LayoutFile.DirectoryName;
                Directory.CreateDirectory(layoutsPath);

                CabinLayout template = CabinLayout.MakeTemplate(data, layoutsPath);
                template.SaveLayout();
                OnTemplateCreatedEventArgs(new TemplateCreatedEventArgs(template));
                IsTemplatingMode = true;
            }
        }

        private void deck_scroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshScrollBarVisibleFlags();
        }

        private void container_decks_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateThumbnailsForLayout();
        }

        private void RefreshScrollBarVisibleFlags()
        {
            InvokePropertyChanged(nameof(IsHorizontalScrollBarVisible));
            InvokePropertyChanged(nameof(IsVerticalScrollBarVisible));
        }

        protected virtual void OnLayoutRegenerated(EventArgs e)
        {
            LayoutRegenerated?.Invoke(this, e);
            OnChanged(new ChangedEventArgs(HasUnsavedChanges));
        }

        protected virtual void OnLayoutReloaded(EventArgs e)
        {
            LayoutReloaded?.Invoke(this, e);
        }

        protected virtual void OnLayoutLoading(EventArgs e)
        {
            LayoutLoading?.Invoke(this, e);
        }

        protected virtual void OnChanged(ChangedEventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        protected virtual void OnTemplatingModeToggled(EventArgs e)
        {
            TemplatingModeToggled?.Invoke(this, e);
        }

        protected virtual void OnTemplateCreatedEventArgs(TemplateCreatedEventArgs e)
        {
            TemplateCreated?.Invoke(this, e);
        }

        private void DeleteLayout_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDialog dialog = new ConfirmationDialog(!IsTemplatingMode ? "Delete cabin layout" : "Delete template",
                "Are you sure you want to delete your " + (!IsTemplatingMode ? "cabin layout" : "template") + "? This action cannot be undone!", 
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

        private void EditCabinLayoutName_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.EditLayoutNameRequested, CabinLayout);
        }

        protected virtual void OnSelectedSlotsChanged(SelectedSlotsChangedEventArgs e)
        {
            SelectedSlotsChanged?.Invoke(this, e);
        }

        private void deck_scroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Util.IsShiftDown())
            {
                deck_scroll.ScrollToHorizontalOffset(deck_scroll.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void deck_scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((App.GuidedTour?.IsExplainingDeckIssueCards ?? false) && 
                (App.GuidedTour?.WaitForUIElement ?? false))
            {
                App.GuidedTour.ContinueTour(true);
            }
        }
    }
}
