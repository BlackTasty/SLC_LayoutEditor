using Newtonsoft.Json.Linq;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Patcher;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
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
        private IDialog mDialog;

        private FrameworkElement mContent;
        private LayoutEditor editor;
        private string mCabinLayoutName;
        private bool mHasUnsavedChanges;

        private bool mIsSearching;

        public string Title => string.Format("SLC Layout Editor ({0}) {1} {2}", App.GetVersionText(), 
            mCabinLayoutName != null ? "- " + mCabinLayoutName : "", mHasUnsavedChanges ? "(UNSAVED CHANGES)" : "");

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
                if (mContent is LayoutEditor oldEditor)
                {
                    this.editor = oldEditor;
                    oldEditor.CabinLayoutSelected -= Editor_CabinLayoutSelected;
                }

                mContent = value;

                if (value is LayoutEditor editor)
                {
                    editor.CabinLayoutSelected += Editor_CabinLayoutSelected;
                    editor.Changed += Editor_LayoutChanged;
                }

                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsViewNotEditor));
            }
        }

        public bool IsViewNotEditor => !(mContent is LayoutEditor);


        public MainViewModel()
        {
            if (!App.Settings.WelcomeScreenShown)
            {
                ShowWelcomeScreen();
            }
            else
            {
                LayoutEditor editor = new LayoutEditor();
                editor.CabinLayoutSelected += Editor_CabinLayoutSelected;
                editor.Changed += Editor_LayoutChanged;
                mContent = editor;
            }


            Mediator.Instance.Register(o =>
            {
                if (o is IDialog dialog)
                {
                    Dialog = dialog;
                }
                else
                {
                    Dialog = new ConfirmationDialog("Error setting dialog!", "The supplied dialog control does not inherit from IDialog!", Core.Enum.DialogType.OK);
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
            Content = editor;
        }

        public void ShowChangelog()
        {
            Dialog = new ChangelogDialog();
        }

        public void ShowChangelogIfUpdated()
        {
            if (App.Settings.LastVersionChangelogShown < App.Patcher.VersionData.VersionNumber &&
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
            Mediator.Instance.NotifyColleagues(ViewModelMessage.DialogOpening, dialog);
        }

        private void Dialog_DialogClosing(object sender, DialogClosingEventArgs e)
        {
            Dialog = null;
        }

        private void Editor_LayoutChanged(object sender, ChangedEventArgs e)
        {
            HasUnsavedChanges = e.UnsavedChanges;
        }

        private void Editor_CabinLayoutSelected(object sender, CabinLayoutSelectedEventArgs e)
        {
            CabinLayoutName = e.CabinLayoutName;
        }
    }
}
