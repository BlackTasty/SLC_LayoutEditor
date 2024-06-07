using Newtonsoft.Json.Linq;
using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.Core.Patcher;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Commands;
using SLC_LayoutEditor.ViewModel.Commands.SlotType;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel
{
    class MainViewModel : MementoViewModel
    {
        public event EventHandler<HistoryChangedEventArgs<CabinHistoryEntry>> HistoryChanged;

        private Queue<IDialog> queuedDialogs = new Queue<IDialog>();
        private IDialog mDialog;

        private FrameworkElement mContent;
        private LayoutEditor editor;
        private string mCabinLayoutName;
        private bool mIsTemplate;
        private bool mHasUnsavedChanges;
        private string mStateToggleButtonContent = FixedValues.MAXIMIZE_ICON;
        private bool mIsMaximized;
        private bool mIsGuideOpen;

        private bool mIsSearching;

        #region Commands
        public CreateCabinLayoutCommand CreateCabinLayoutCommand => CommandInterface.CreateCabinLayoutCommand;

        public CreateTemplateCommand CreateTemplateCommand => CommandInterface.CreateTemplateCommand;

        public CreateAircraftCommand CreateAircraftCommand => CommandInterface.CreateAircraftCommand;

        public MakeTemplateCommand MakeTemplateCommand => CommandInterface.MakeTemplateCommand;

        public OpenLayoutFolderCommand OpenLayoutFolderCommand => CommandInterface.OpenLayoutFolderCommand;

        public OpenLayoutInTextEditor OpenLayoutInTextEditor => CommandInterface.OpenLayoutInTextEditor;

        public ReloadLayoutCommand ReloadLayoutCommand => CommandInterface.ReloadLayoutCommand;

        public RenameLayoutCommand RenameLayoutCommand => CommandInterface.RenameLayoutCommand;

        public SaveLayoutCommand SaveLayoutCommand => CommandInterface.SaveLayoutCommand;

        public SaveLayoutAsCommand SaveLayoutAsCommand => CommandInterface.SaveLayoutAsCommand;

        public SelectAllSlotsCommand SelectAllSlotsCommand => CommandInterface.SelectAllSlotsCommand;

        public SelectAllSlotsOnDeckCommand SelectAllSlotsOnDeckCommand => CommandInterface.SelectAllSlotsOnDeckCommand;

        public ShowKeybindsWindowCommand ShowKeybindsWindowCommand => CommandInterface.ShowKeybindsWindowCommand;

        public CancelDialogCommand CancelDialogCommand => CommandInterface.CancelDialogCommand;

        public UndoCommand UndoCommand => CommandInterface.UndoCommand;

        public RedoCommand RedoCommand => CommandInterface.RedoCommand;

        public UndoUntilCommand UndoUntilCommand => CommandInterface.UndoUntilCommand;

        public RedoUntilCommand RedoUntilCommand => CommandInterface.RedoUntilCommand;

        #region Slot type commands
        public SlotTypeAisleCommand SlotTypeAisleCommand => CommandInterface.SlotTypeAisleCommand;

        public SlotTypeBusinessClassCommand SlotTypeBusinessClassCommand => CommandInterface.SlotTypeBusinessClassCommand;

        public SlotTypeCateringDoorCommand SlotTypeCateringDoorCommand => CommandInterface.SlotTypeCateringDoorCommand;

        public SlotTypeDoorCommand SlotTypeDoorCommand => CommandInterface.SlotTypeDoorCommand;

        public SlotTypeCockpitCommand SlotTypeCockpitCommand => CommandInterface.SlotTypeCockpitCommand;

        public SlotTypeEconomyClassCommand SlotTypeEconomyClassCommand => CommandInterface.SlotTypeEconomyClassCommand;

        public SlotTypeFirstClassCommand SlotTypeFirstClassCommand => CommandInterface.SlotTypeFirstClassCommand;

        public SlotTypeGalleyCommand SlotTypeGalleyCommand => CommandInterface.SlotTypeGalleyCommand;

        public SlotTypeIntercomCommand SlotTypeIntercomCommand => CommandInterface.SlotTypeIntercomCommand;

        public SlotTypeKitchenCommand SlotTypeKitchenCommand => CommandInterface.SlotTypeKitchenCommand;

        public SlotTypeLoadingBayCommand SlotTypeLoadingBayCommand => CommandInterface.SlotTypeLoadingBayCommand;

        public SlotTypePremiumClassCommand SlotTypePremiumClassCommand => CommandInterface.SlotTypePremiumClassCommand;

        public SlotTypeServiceEndCommand SlotTypeServiceEndCommand => CommandInterface.SlotTypeServiceEndCommand;

        public SlotTypeServiceStartCommand SlotTypeServiceStartCommand => CommandInterface.SlotTypeServiceStartCommand;

        public SlotTypeStairwayCommand SlotTypeStairwayCommand => CommandInterface.SlotTypeStairwayCommand;

        public SlotTypeSupersonicClassCommand SlotTypeSupersonicClassCommand => CommandInterface.SlotTypeSupersonicClassCommand;

        public SlotTypeToiletCommand SlotTypeToiletCommand => CommandInterface.SlotTypeToiletCommand;

        public SlotTypeUnavailableSeatCommand SlotTypeUnavailableSeatCommand => CommandInterface.SlotTypeUnavailableSeatCommand;

        public SlotTypeWallCommand SlotTypeWallCommand => CommandInterface.SlotTypeWallCommand;
        #endregion
        #endregion

        /*public string Title => string.Format("SLC Layout Editor ({0}) {1} {2}", App.GetVersionText(),
            mCabinLayoutName != null ? "- " + mCabinLayoutName : "", mHasUnsavedChanges ? "(UNSAVED CHANGES)" : "");*/

        public string Title => string.Format("SLC Layout Editor ({0})", App.GetVersionText());

        public string CabinLayoutName
        {
            get => mCabinLayoutName;
            private set
            {
                mCabinLayoutName = value;
                HasUnsavedChanges = false;
                InvokePropertyChanged();
            }
        }

        public bool IsTemplate
        {
            get => mIsTemplate;
            private set
            {
                mIsTemplate = value;
                InvokePropertyChanged();
            }
        }

        public string StateToggleButtonContent
        {
            get => mStateToggleButtonContent;
            set
            {
                mStateToggleButtonContent = value;
                InvokePropertyChanged();
            }
        }

        public bool IsMaximized
        {
            get => mIsMaximized;
            set
            {
                mIsMaximized = value;
                InvokePropertyChanged();
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Window_StateChanged, value);
            }
        }

        public bool HasUnsavedChanges
        {
            get => mHasUnsavedChanges;
            private set
            {
                mHasUnsavedChanges = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(Title));
            }
        }

        public bool IsSearching
        {
            get => mIsSearching;
            set
            {
                mIsSearching = value;
                InvokePropertyChanged();
            }
        }

        public IDialog Dialog
        {
            get => mDialog;
            set
            {
                if (mDialog != null)
                {
                    mDialog.DialogClosing -= Dialog_DialogClosing;
                }
                mDialog = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsDialogOpen));

                if (mDialog != null)
                {
                    mDialog.DialogClosing += Dialog_DialogClosing;
                }

                App.IsDialogOpen = value != null;

                if (mContent is LayoutEditor layoutEditor)
                {
                    layoutEditor.RenderAdorners();
                }
            }
        }

        public bool IsDialogOpen => mDialog != null;

        public FrameworkElement Content
        {
            get => mContent;
            set
            {
                LayoutEditor newValue = value as LayoutEditor;

                if (mContent is LayoutEditor oldEditor)
                {
                    if (newValue == null)
                    {
                        this.editor.DeselectAllSlots();
                    }

                    this.editor = oldEditor;
                    oldEditor.CabinLayoutSelected -= Editor_CabinLayoutSelected;
                    oldEditor.Changed -= Editor_LayoutChanged;
                    oldEditor.TourRunningStateChanged -= Editor_TourRunningStateChanged;
                }

                mContent = value;

                if (newValue != null)
                {
                    newValue.CabinLayoutSelected += Editor_CabinLayoutSelected;
                    newValue.Changed += Editor_LayoutChanged;
                    newValue.TourRunningStateChanged += Editor_TourRunningStateChanged;
                }

                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsViewNotEditor));
            }
        }

        public LayoutEditorViewModel EditorViewModel => editor != null ? editor.DataContext as LayoutEditorViewModel : null;

        public bool IsLayoutOpened => EditorViewModel?.ActiveLayout != null;

        public bool AllowHistoryCommands => !IsViewNotEditor && !IsDialogOpen && !mIsGuideOpen;

        public bool IsViewNotEditor => !(mContent is LayoutEditor) && App.Settings.WelcomeScreenShown;

        public bool IsTourRunning => App.GuidedTour?.IsTourRunning ?? false;

        public string TourStepCategory => CurrentTourStep > 1 ? App.GuidedTour?.TourStepCategory : "Welcome";

        public int CurrentTourStep => App.GuidedTour?.CurrentStepNumber ?? 0;

        public bool IsNotFirstTourStep => CurrentTourStep > 1;

        public bool IsNotLatestTourStep => CurrentTourStep < App.GuidedTour?.HighestAchievedStep && CurrentTourStep < MaxSteps;

        public int MaxSteps => FixedValues.TOTAL_TOUR_STEPS;

        /// <summary>
        /// Defines if any <see cref="LiveGuideAdorner"/> is currently open
        /// </summary>
        public bool IsGuideOpen
        {
            get => mIsGuideOpen;
            set
            {
                mIsGuideOpen = value;
                InvokePropertyChanged();
            }
        }

        public MainViewModel()
        {
            if (!App.Settings.WelcomeScreenShown)
            {
                ShowWelcomeScreen();
            }
            else
            {
                mContent = GetEditor();
            }

            History.HistoryApplying += History_HistoryApplying;

            Mediator.Instance.Register(o =>
            {
                if (o is IDialog dialog)
                {
                    ShowDialog(dialog);
                }
                else
                {
                    ShowDialog(new ConfirmationDialog("Error setting dialog!", "The supplied dialog control does not inherit from IDialog!", Core.Enum.DialogType.OK));
                }
            }, ViewModelMessage.DialogOpening);

            Mediator.Instance.Register(o =>
            {
                ReturnToEditor();
            }, ViewModelMessage.SettingsSaved);

            Mediator.Instance.Register(o =>
            {
                IsSearching = (bool)o;
            }, ViewModelMessage.Patcher_IsSearchingChanged);
        }

        private void History_HistoryApplying(object sender, HistoryApplyingEventArgs<CabinHistoryEntry> e)
        {
            EditorViewModel.ActiveLayout.ApplyHistoryEntry(e.HistoryEntry, e.IsUndo);
        }

        protected override void History_Changed(object sender, HistoryChangedEventArgs<CabinHistoryEntry> e)
        {
            base.History_Changed(sender, e);
            if (!e.IsClear && e.PoppedHistory.Count() == 1)
            {
                EditorViewModel.ActiveLayout.ToggleIssueChecking(true);
            }
            OnHistoryChanged(e);
        }

        protected override void History_HistoryChanging(object sender, EventArgs e)
        {
            EditorViewModel.ActiveLayout.ToggleIssueChecking(false);
        }

        public void ShowWelcomeScreen()
        {
            WelcomeScreen welcome = new WelcomeScreen();
            welcome.WelcomeConfirmed += Welcome_WelcomeConfirmed;
            Content = welcome;
        }

        public void ShowSettings()
        {
            Settings settings = new Settings();
            Content = settings;
        }

        public void ReturnToEditor()
        {
            Content = GetEditor();
        }

        public void ShowChangelog()
        {
            ShowDialog(new ChangelogDialog());
        }

        public void ShowAbout()
        {
            ShowDialog(new About());
        }

        public void ShowChangelogIfUpdated()
        {
            if (App.Settings.LastVersionChangelogShown != App.Patcher.VersionData.VersionNumber &&
                App.Settings.ShowChangesAfterUpdate)
            {
                ShowChangelog();
                App.Settings.LastVersionChangelogShown = App.Patcher.VersionData.VersionNumber;
                App.SaveAppSettings();
            }
        }

        public bool CheckUnsavedChanges(bool isClosing)
        {
            if (mContent is LayoutEditor editor)
            {
                return editor.CheckUnsavedChanges(isClosing);
            }
            else if (this.editor != null)
            {
                return this.editor.CheckUnsavedChanges(isClosing);
            }

            return false;
        }

        public void RememberLayout()
        {
            if (Content is LayoutEditor editor)
            {
                editor.RememberLayout();
            }
            else
            {
                this.editor?.RememberLayout();
            }
        }

        public void UpdateTourStepper()
        {
            InvokePropertyChanged(nameof(TourStepCategory));
            InvokePropertyChanged(nameof(CurrentTourStep));
            InvokePropertyChanged(nameof(IsNotFirstTourStep));
            InvokePropertyChanged(nameof(IsNotLatestTourStep));
        }

        private void CheckForSnapshots()
        {
            List<CabinLayout> snapshots = new List<CabinLayout>();

            
        }

        private LayoutEditor GetEditor()
        {
            if (this.editor != null)
            {
                return this.editor;
            }
            editor = new LayoutEditor();
            editor.CabinLayoutSelected += Editor_CabinLayoutSelected;
            editor.Changed += Editor_LayoutChanged;
            editor.TourRunningStateChanged += Editor_TourRunningStateChanged;
            InvokePropertyChanged(nameof(EditorViewModel));

            return editor;
        }

        private void Welcome_WelcomeConfirmed(object sender, EventArgs e)
        {
            if (mContent is WelcomeScreen welcome)
            {
                welcome.WelcomeConfirmed -= Welcome_WelcomeConfirmed;
            }

            Content = new LayoutEditor();
        }

        private void ShowDialog(IDialog dialog)
        {
            if (mDialog.IsSameDialogType(dialog))
            {
                return;
            }
            if (mDialog == null)
            {
                Dialog = dialog;
            }
            else
            {
                if (queuedDialogs.Any(x => x.IsSameDialogType(dialog)))
                {
                    return;
                }
                queuedDialogs.Enqueue(dialog);
            }
        }

        private void Dialog_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            if (queuedDialogs.Count == 0)
            {
                Dialog = null;
            }
            else
            {
                Dialog = queuedDialogs.Dequeue();
            }
        }

        private void Editor_LayoutChanged(object sender, ChangedEventArgs e)
        {
            HasUnsavedChanges = e.UnsavedChanges;
        }

        private void Editor_CabinLayoutSelected(object sender, CabinLayoutSelectedEventArgs e)
        {
            CabinLayoutName = e.CabinLayoutName;
            IsTemplate = e.IsTemplate;
            InvokePropertyChanged(nameof(IsLayoutOpened));
            History.Clear();
        }

        private void Editor_TourRunningStateChanged(object sender, EventArgs e)
        {
            UpdateTourStepper();
            InvokePropertyChanged(nameof(IsTourRunning));
        }

        protected virtual void OnHistoryChanged(HistoryChangedEventArgs<CabinHistoryEntry> e)
        {
            HistoryChanged?.Invoke(this, e);
        }
    }
}
