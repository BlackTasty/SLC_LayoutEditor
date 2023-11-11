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
        private FrameworkElement mContent;
        private LayoutEditor editor;
        private string mCabinLayoutName;
        private bool mHasUnsavedChanges;
        private bool mIsDialogOpen;

        #region Updater variables
        private UpdateManager updateManager;

        private string mUpdateText = "Idle";
        private bool mIsSearching;
        private bool mIsDownloading;
        private double mDownloadSize;
        private double mDownloadCurrent;
        private bool mIsUpdateReady;
        #endregion

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
        
        public bool IsDialogOpen
        {
            get => mIsDialogOpen;
            private set
            {
                mIsDialogOpen = value;
                InvokePropertyChanged();
            }
        }

        private void Editor_LayoutChanged(object sender, ChangedEventArgs e)
        {
            HasUnsavedChanges = e.UnsavedChanges;
        }

        private void Editor_CabinLayoutSelected(object sender, CabinLayoutSelectedEventArgs e)
        {
            CabinLayoutName = e.CabinLayoutName;
        }

        public bool IsViewNotEditor => !(mContent is LayoutEditor);

        #region Updater properties
        public UpdateManager UpdateManager => updateManager;

        public UpdateStatus UpdateStatus
        {
            get => updateManager?.Status ?? UpdateStatus.IDLE;
            set => updateManager.Status = value;
        }

        public string UpdateText
        {
            get => mUpdateText;
            set
            {
                mUpdateText = value;
                InvokePropertyChanged();
            }
        }

        public bool IsSearching
        {
            get
            {
                if (!mIsSearching)
                {
                    return mIsUpdateReady;
                }
                else
                {
                    return mIsSearching;
                }
            }
            set
            {
                mIsSearching = value;
                if (!value && !mIsUpdateReady)
                {
                    new Thread(() => {
                        Thread.Sleep(3000);
                        InvokePropertyChanged();
                    }).Start();
                }
                else
                {
                    InvokePropertyChanged();
                }
            }
        }

        public bool IsDownloading
        {
            get => mIsDownloading;
            set
            {
                mIsDownloading = value;
                InvokePropertyChanged();
            }
        }

        public bool IsUpdateReady
        {
            get => mIsUpdateReady;
            set
            {
                mIsUpdateReady = value;
                InvokePropertyChanged();
            }
        }

        public double DownloadSize
        {
            get => mDownloadSize;
            set
            {
                mDownloadSize = value;
                InvokePropertyChanged();
            }
        }

        public double DownloadCurrent
        {
            get => mDownloadCurrent;
            set
            {
                mDownloadCurrent = value;
                InvokePropertyChanged();
            }
        }
        #endregion

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

            if (!App.IsDesignMode)
            {
                updateManager = new UpdateManager();
                updateManager.DownloadProgressChanged += UpdateManager_DownloadProgressChanged;
                updateManager.UpdateFailed += UpdateManager_UpdateFailed;
                updateManager.SearchStatusChanged += UpdateManager_SearchStatusChanged;
                updateManager.StatusChanged += UpdateManager_StatusChanged;
            }


            Mediator.Instance.Register(o =>
            {
                IsDialogOpen = true;
            }, ViewModelMessage.DialogOpening);

            Mediator.Instance.Register(o =>
            {
                IsDialogOpen = false;
            }, ViewModelMessage.DialogClosing);
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
            if (mContent is LayoutEditor editor)
            {
                editor.ShowChangelog();
            }
        }

        public void ShowChangelogIfUpdated()
        {
            if (App.Settings.LastVersionChangelogShown < UpdateManager.VersionNumber &&
                App.Settings.ShowChangesAfterUpdate)
            {
                ShowChangelog();
            }

            App.Settings.LastVersionChangelogShown = UpdateManager.VersionNumber;
            App.SaveAppSettings();
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

        private void UpdateManager_StatusChanged(object sender, UpdateStatus e)
        {
            InvokePropertyChanged(nameof(UpdateStatus));

            switch (e)
            {
                case UpdateStatus.IDLE:
                    UpdateText = "Idle";
                    break;
                case UpdateStatus.SEARCHING:
                    IsSearching = true;
                    UpdateText = "Searching for updates...";
                    break;
                case UpdateStatus.UPDATES_FOUND:
                    UpdateText = "Updates found!";
                    IsUpdateReady = true;
                    break;
                case UpdateStatus.DOWNLOADING:
                    IsUpdateReady = false;
                    IsDownloading = true;
                    UpdateText = "Downloading update...";
                    break;
                case UpdateStatus.EXTRACTING:
                case UpdateStatus.INSTALLING:
                    IsDownloading = false;
                    UpdateText = "Extracting files...";
                    break;
                case UpdateStatus.READY:
                    UpdateText = "Update installed!";
                    Process.Start(AppDomain.CurrentDomain.BaseDirectory + "SLC_LayoutEditor.exe");
                    Application.Current.MainWindow.Close();
                    break;
                case UpdateStatus.UPTODATE:
                    UpdateText = "Everything is up-to-date!";
                    break;
            }
        }

        private void UpdateManager_SearchStatusChanged(object sender, bool e)
        {
            IsSearching = e;
        }

        private void UpdateManager_UpdateFailed(object sender, UpdateFailedEventArgs e)
        {
            UpdateText = e.ErrorMessage;
        }

        private void UpdateManager_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            UpdateText = "Downloading - " +
                Math.Round((e.BytesReceived / 1024d) / 1024d, 2).ToString("0.00") + " from " +
                Math.Round((e.TotalBytesToReceive / 1024d) / 1024d, 2).ToString("0.00") +
                " (" + updateManager.CalculateSpeed(e.BytesReceived) + ")";
            DownloadSize = e.TotalBytesToReceive;
            DownloadCurrent = e.BytesReceived;
        }
    }
}
