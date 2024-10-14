using SLC_LayoutEditor.Core.Patcher;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using Tasty.Logging;
using Tasty.ViewModel;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel
{
    internal class UpdaterViewModel : ViewModelBase
    {
        private string mUpdateText = "Idle";
        private bool mIsSearching;
        private bool mShowProgressBar;
        private double mDownloadSize;
        private double mDownloadCurrent;
        private bool mIsUpdateReady;

        private bool mIsMouseOver;
        private bool mIsPressed;
        private bool mIsInteractable;

        #region Updater properties
        public UpdateManager Patcher => App.Patcher;

        public UpdateStatus UpdateStatus
        {
            get => Patcher?.Status ?? UpdateStatus.IDLE;
            set => Patcher.Status = value;
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
                InvokePropertyChanged();
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Patcher_IsSearchingChanged, value);
            }
        }

        public bool IsReady => UpdateStatus == UpdateStatus.READY;

        public bool ShowProgressBar
        {
            get => mShowProgressBar;
            set
            {
                mShowProgressBar = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowButton
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

        public bool IsIndeterminateUpdateProgress => UpdateStatus == UpdateStatus.SEARCHING ||
            UpdateStatus == UpdateStatus.EXTRACTING ||
            UpdateStatus == UpdateStatus.INSTALLING;
        #endregion

        public bool IsMouseOver
        {
            get => mIsMouseOver;
            set
            {
                mIsMouseOver = value;
                InvokePropertyChanged();
            }
        }

        public bool IsPressed
        {
            get => mIsPressed;
            set
            {
                mIsPressed = value;
                InvokePropertyChanged();
            }
        }

        public bool IsInteractable
        {
            get => mIsInteractable;
            set
            {
                mIsInteractable = value;
                InvokePropertyChanged();
            }
        }

        public UpdaterViewModel()
        {
            if (!App.IsDesignMode)
            {
                Patcher.DownloadProgressChanged += UpdateManager_DownloadProgressChanged;
                Patcher.UpdateFailed += UpdateManager_UpdateFailed;
                Patcher.SearchStatusChanged += UpdateManager_SearchStatusChanged;
                Patcher.StatusChanged += UpdateManager_StatusChanged;
            }

            if (Patcher != null)
            {
                RefreshStatus();
            }
        }

        public void ForceRefreshProperties()
        {
            RefreshPatcherProperties();
            InvokePropertyChanged(nameof(UpdateText));
            InvokePropertyChanged(nameof(IsSearching));
            InvokePropertyChanged(nameof(DownloadSize));
            InvokePropertyChanged(nameof(DownloadCurrent));
        }

        private void UpdateManager_StatusChanged(object sender, UpdateStatus e)
        {
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            Logger.Default.WriteLog("Patcher status changed. New status: {0}", LogType.DEBUG, UpdateStatus.ToString());

            if (IsIndeterminateUpdateProgress)
            {
                DownloadCurrent = 0;
                DownloadSize = double.MaxValue;
            }

            switch (UpdateStatus)
            {
                case UpdateStatus.IDLE:
                    ShowButton = false;
                    ShowProgressBar = false;
                    IsInteractable = false;
                    UpdateText = "Idle";
                    break;
                case UpdateStatus.SEARCHING:
                    ShowProgressBar = true;
                    //ShowButton = false;
                    IsSearching = true;
                    UpdateText = "Searching for updates...";
                    break;
                case UpdateStatus.UPDATES_FOUND:
                    ShowButton = true;
                    ShowProgressBar = false;
                    IsInteractable = true;
                    UpdateText = "Updates ready to download";
                    break;
                case UpdateStatus.DOWNLOADING:
                    ShowButton = false;
                    ShowProgressBar = true;
                    IsInteractable = false;
                    UpdateText = "Starting download...";
                    break;
                case UpdateStatus.EXTRACTING:
                case UpdateStatus.INSTALLING:
                    UpdateText = "Extracting files...";
                    break;
                case UpdateStatus.READY:
                    ShowButton = true;
                    ShowProgressBar = false;
                    IsInteractable = true;
                    UpdateText = "Update installed, restart the editor";
                    break;
                case UpdateStatus.UPTODATE:
                    ShowButton = true;
                    ShowProgressBar = false;
                    UpdateText = "Everything is up-to-date!";
                    break;
            }

            RefreshPatcherProperties();
        }

        private void UpdateManager_SearchStatusChanged(object sender, bool e)
        {
            IsSearching = e;
        }

        private void RefreshPatcherProperties()
        {
            InvokePropertyChanged(nameof(UpdateStatus));
            InvokePropertyChanged(nameof(IsIndeterminateUpdateProgress));
            InvokePropertyChanged(nameof(ShowProgressBar));
            InvokePropertyChanged(nameof(ShowButton));
            InvokePropertyChanged(nameof(IsReady));
        }

        private void UpdateManager_UpdateFailed(object sender, UpdateFailedEventArgs e)
        {
            UpdateText = e.ErrorMessage;
        }

        private void UpdateManager_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            UpdateText = "Downloading - " +
                Math.Round((e.BytesReceived / 1024d) / 1024d, 2).ToString("0.00") + " MB from " +
                Math.Round((e.TotalBytesToReceive / 1024d) / 1024d, 2).ToString("0.00") +
                " MB (" + Patcher.CalculateSpeed(e.BytesReceived) + ")";
            DownloadSize = e.TotalBytesToReceive;
            DownloadCurrent = e.BytesReceived;
        }
    }
}
