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
using System.Windows.Shapes;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for CabinDeckControl.xaml
    /// </summary>
    public partial class CabinDeckControl : DockPanel, INotifyPropertyChanged, IDisposable
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
        public event EventHandler<SelectedDeckChangedEventArgs> SelectedDeckChanged;

        private CabinDeck currentRemoveTarget;

        private DeckLayoutControl selectedDeck;

        #region CabinLayout property
        public CabinLayout CabinLayout
        {
            get { return (CabinLayout)GetValue(CabinLayoutProperty); }
            set { SetValue(CabinLayoutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CabinLayout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinLayoutProperty =
            DependencyProperty.Register("CabinLayout", typeof(CabinLayout), typeof(CabinDeckControl), new PropertyMetadata(null,
                OnCabinLayoutChanged));

        private static void OnCabinLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is CabinDeckControl control)
            {
                CabinLayout oldLayout = e.OldValue as CabinLayout;
                CabinLayout newLayout = e.NewValue as CabinLayout;
                Logger.Default.WriteLog("Layout has changed for UI. Old value: {0}; New value: {1}", 
                    GetCabinLayoutValueForLog(oldLayout), GetCabinLayoutValueForLog(newLayout));
                control.RefreshCabinLayout();
            }
        }

        private static string GetCabinLayoutValueForLog(CabinLayout cabinLayout)
        {
            return cabinLayout != null ?
                string.Format("{0} (type: {1})", cabinLayout.LayoutName, cabinLayout.IsTemplate ? "template" : "layout") :
                "Unset";
        }
        #endregion

        #region SelectedCabinSlots property
        public List<CabinSlot> SelectedCabinSlots
        {
            get { return (List<CabinSlot>)GetValue(SelectedCabinSlotsProperty); }
            set { SetValue(SelectedCabinSlotsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCabinSlots.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCabinSlotsProperty =
            DependencyProperty.Register("SelectedCabinSlots", typeof(List<CabinSlot>), typeof(CabinDeckControl), new PropertyMetadata(new List<CabinSlot>()));
        #endregion

        public ContextMenu GuideMenu
        {
            get { return (ContextMenu)GetValue(GuideMenuProperty); }
            set { SetValue(GuideMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GuideMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GuideMenuProperty =
            DependencyProperty.Register("GuideMenu", typeof(ContextMenu), typeof(CabinDeckControl), new PropertyMetadata(null));

        public string LayoutOverviewTitle => CabinLayout != null ?
            string.Format("{0} {1}", CabinLayout.LayoutName, HasUnsavedChanges ? "*" : "") :
            "No layout selected";

        public bool HasUnsavedChanges => Util.HasLayoutChanged(CabinLayout);

        public bool IsHorizontalScrollBarVisible => deck_scroll.ComputedHorizontalScrollBarVisibility == Visibility.Visible;

        public bool IsVerticalScrollBarVisible => deck_scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible;

        #region SelectedCabinSlotFloor property
        public int SelectedCabinSlotFloor
        {
            get { return (int)GetValue(SelectedCabinSlotFloorProperty); }
            set { SetValue(SelectedCabinSlotFloorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCabinSlotFloor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCabinSlotFloorProperty =
            DependencyProperty.Register("SelectedCabinSlotFloor", typeof(int), typeof(CabinDeckControl), new PropertyMetadata(0));
        #endregion

        #region SelectedMultiSlotTypeIndex properties
        public int SelectedMultiSlotTypeIndex
        {
            get { return (int)GetValue(SelectedMultiSlotTypeIndexProperty); }
            set { SetValue(SelectedMultiSlotTypeIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedMultiSlotTypeIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedMultiSlotTypeIndexProperty =
            DependencyProperty.Register("SelectedMultiSlotTypeIndex", typeof(int), typeof(CabinDeckControl), new PropertyMetadata(-1));
        #endregion

        public bool IsTemplatingMode
        {
            get { return (bool)GetValue(IsTemplatingModeProperty); }
            set { SetValue(IsTemplatingModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTemplatingMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTemplatingModeProperty =
            DependencyProperty.Register("IsTemplatingMode", typeof(bool), typeof(CabinDeckControl), new PropertyMetadata(false, OnIsTemplatingModeChanged));

        public bool IsSidebarOpen
        {
            get { return (bool)GetValue(IsSidebarOpenProperty); }
            set { SetValue(IsSidebarOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSidebarOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSidebarOpenProperty =
            DependencyProperty.Register("IsSidebarOpen", typeof(bool), typeof(CabinDeckControl), new PropertyMetadata(true));

        private static void OnIsTemplatingModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && sender is CabinDeckControl control)
            {
                control.OnTemplatingModeToggled(EventArgs.Empty);
            }
        }

        public CabinDeckControl()
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
        }

        public void GenerateThumbnailForLayout(bool overwrite = false)
        {
            if (CabinLayout != null)
            {
                Logger.Default.WriteLog("Generating thumbnail for {0} \"{1}\"...", CabinLayout.IsTemplate ? "template" : "layout", CabinLayout.LayoutName);
                Directory.CreateDirectory(CabinLayout.ThumbnailDirectory);

                foreach (DeckLayoutControl deckLayoutControl in container_decks.Children.OfType<DeckLayoutControl>())
                {
                    deckLayoutControl.GenerateThumbnailForDeck(CabinLayout.ThumbnailDirectory, overwrite);
                }
                Logger.Default.WriteLog("{0} saved successfully!", CabinLayout.IsTemplate ? "template" : "layout");
            }
        }

        public void RenderAdorners()
        {
            foreach (DeckLayoutControl deckLayoutControl in container_decks.Children.OfType<DeckLayoutControl>())
            {
                deckLayoutControl.RenderAdorners();
            }
        }

        public UIElement GetDeckIssueElement()
        {
            return container_decks.Children.OfType<DeckLayoutControl>().FirstOrDefault().cards_deckIssues;
        }

        public void Dispose()
        {
            if (CabinLayout != null)
            {
                CabinLayout.CabinSlotsChanged -= CabinLayout_CabinSlotsChanged;
                CabinLayout.CabinDeckCountChanged -= CabinLayout_CabinDeckCountChanged;

                foreach (DeckLayoutControl deckLayoutControl in container_decks?.Children.OfType<DeckLayoutControl>())
                {
                    deckLayoutControl.LayoutRegenerated -= CabinDeckControl_LayoutRegenerated;
                    deckLayoutControl.RemoveDeckClicked -= CabinDeckControl_RemoveDeckClicked;

                    deckLayoutControl.SizeChanged -= CabinDeckControl_SizeChanged;
                }
            }
        }

        private void RefreshCabinLayout()
        {
            OnLayoutLoading(EventArgs.Empty);

            //Unhook events before clearing container for deck layout controls
            foreach (DeckLayoutControl cabinDeckControl in container_decks.Children.OfType<DeckLayoutControl>())
            {
                cabinDeckControl.LayoutRegenerated -= CabinDeckControl_LayoutRegenerated;
                cabinDeckControl.RemoveDeckClicked -= CabinDeckControl_RemoveDeckClicked;
                //cabinDeckControl.LayoutLoading -= CabinDeckControl_LayoutLoading;

                cabinDeckControl.SizeChanged -= CabinDeckControl_SizeChanged;
                cabinDeckControl.DeckRendered -= CabinDeckControl_DeckRendered;
            }
            container_decks.Children.Clear();

            if (CabinLayout != null)
            {
                Logger.Default.WriteLog("Refreshing view for {0} \"{1}\"...", CabinLayout.IsTemplate ? "template" : "layout", CabinLayout.LayoutName);
                CabinLayout.CabinSlotsChanged += CabinLayout_CabinSlotsChanged;
                CabinLayout.CabinDeckCountChanged += CabinLayout_CabinDeckCountChanged;

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
            DeckLayoutControl cabinDeckControl = new DeckLayoutControl()
            {
                CabinDeck = cabinDeck,
                GuideMenu = GuideMenu
            };

            cabinDeckControl.SelectedSlotsChanged += CabinDeckControl_SelectedSlotsChanged;

            cabinDeckControl.LayoutRegenerated += CabinDeckControl_LayoutRegenerated;
            cabinDeckControl.RemoveDeckClicked += CabinDeckControl_RemoveDeckClicked;
            //cabinDeckControl.LayoutLoading += CabinDeckControl_LayoutLoading;

            cabinDeckControl.SizeChanged += CabinDeckControl_SizeChanged;
            cabinDeckControl.DeckRendered += CabinDeckControl_DeckRendered;
            container_decks.Children.Add(cabinDeckControl);
        }

        private void CabinDeckControl_SelectedSlotsChanged(object sender, SelectedSlotsChangedEventArgs e)
        {
            if (selectedDeck != null &&
                selectedDeck.CabinDeck.Floor != e.DeckControl.CabinDeck.Floor)
            {
                selectedDeck.DeselectSlots();
            }

            SelectedCabinSlots = e.NewSelection.ToList();
            SelectedCabinSlotFloor = e.Floor;
            selectedDeck = e.DeckControl;

            OnSelectedSlotsChanged(e);
            OnSelectedDeckChanged(new SelectedDeckChangedEventArgs(selectedDeck));
        }

        private void CabinDeckControl_DeckRendered(object sender, EventArgs e)
        {
            Directory.CreateDirectory(CabinLayout.ThumbnailDirectory);
            if (sender is DeckLayoutControl deckLayoutControl)
            {
                deckLayoutControl.GenerateThumbnailForDeck(CabinLayout.ThumbnailDirectory, true);
            }
        }

        private void CabinLayout_CabinDeckCountChanged(object sender, EventArgs e)
        {
            RefreshState();
        }

        private void CabinDeckControl_SizeChanged(object sender, EventArgs e)
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
                "Are you sure you want to delete this deck? This action cannot be undone!", DialogType.YesNo);
            currentRemoveTarget = e.Target;

            dialog.DialogClosing += ConfirmRemoveDeck_DialogClosing;

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void ConfirmRemoveDeck_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (e.DialogResult == DialogResultType.Yes)
            {
                CabinLayout.RemoveCabinDeck(currentRemoveTarget);
                CabinLayout.RefreshCalculated();

                if (container_decks.Children.OfType<DeckLayoutControl>()
                        .FirstOrDefault(x => x.CabinDeck.Floor == currentRemoveTarget.Floor) is DeckLayoutControl targetControl)
                {
                    targetControl.LayoutRegenerated -= CabinDeckControl_LayoutRegenerated;
                    targetControl.RemoveDeckClicked -= CabinDeckControl_RemoveDeckClicked;
                    targetControl.SizeChanged -= CabinDeckControl_SizeChanged;
                    targetControl.DeckRendered += CabinDeckControl_DeckRendered;
                    container_decks.Children.Remove(targetControl);
                }
            }

            currentRemoveTarget = null;
        }

        /*private void CabinDeckControl_LayoutLoading(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }*/

        private void AddCabinDeck_Click(object sender, RoutedEventArgs e)
        {
            int rows = 1;
            int columns = 1;

            if (CabinLayout.CabinDecks.Count > 0)
            {
                ConfirmationDialog dialog = new ConfirmationDialog("Match existing layout?",
                    "Should the new deck have matching rows and columns?", DialogType.YesNoCancel);

                dialog.DialogClosing += delegate (object _sender, DialogClosingEventArgs _e) {
                    if (_e.DialogResult == DialogResultType.No)
                    {
                        CreateCabinDeck(rows, columns);
                    }
                    else if (_e.DialogResult == DialogResultType.Yes)
                    {
                        CabinDeck lastDeck = CabinLayout.CabinDecks.LastOrDefault();
                        rows = lastDeck.Rows + 1;
                        columns = lastDeck.Columns + 1;

                        CreateCabinDeck(rows, columns);
                    }
                };

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
            }
            else
            {
                CreateCabinDeck(rows, columns);
            }
        }

        private void CreateCabinDeck(int rows, int columns)
        {
            CabinDeck createdDeck = CabinLayout.AddCabinDeck(new CabinDeck(CabinLayout.CabinDecks.Count + 1, rows, columns));
            AddCabinDeckToUI(createdDeck);

            CabinLayout.RefreshCalculated();
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

                Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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
            SelectedCabinSlots.Clear();
            selectedDeck?.SelectSlots(SelectedCabinSlots);
            CabinLayout.LoadCabinLayoutFromFile(true);
            RefreshCabinLayout();
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

            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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
            GenerateThumbnailForLayout();
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
                "Are you sure you want to delete this " + (!IsTemplatingMode ? "cabin layout" : "template") + "? This action cannot be undone!", 
                DialogType.YesNo);

            dialog.DialogClosing += DeleteLayout_DialogClosing;
            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
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

        protected virtual void OnSelectedDeckChanged(SelectedDeckChangedEventArgs e)
        {
            SelectedDeckChanged?.Invoke(this, e);
        }

        protected virtual void OnSelectedSlotsChanged(SelectedSlotsChangedEventArgs e)
        {
            SelectedSlotsChanged?.Invoke(this, e);
        }
    }
}
