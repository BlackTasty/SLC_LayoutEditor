using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core
{
    public class AppSettings : JsonFile<AppSettings>
    {
        private string mCabinLayoutsReadoutPath; //usually "C:/ProgramFiles (x86)/Lanilogic/Self Loading Cargo/CabinLayouts"
        private string mCabinLayoutsEditPath;

        #region Editor toggles
        private bool mShowWarningWhenIssuesPresent = true;

        private bool mOpenFoldersAfterSaving = true;
        private bool mOpenFolderWithEditedLayout = true;
        private bool mOpenSLCTargetFolder = true;
        #endregion

        #region Utility toggles
        private bool mCopyLayoutsAfterSave;
        private bool mRunCommandPromptHidden =
#if DEBUG
            false;
#else
            true;
#endif
        private bool mOnlyPromptOnceForPrivileges = true;
        #endregion

        #region Updater settings
        private bool mAutoSearchForUpdates = true;
        private bool mShowChangesAfterUpdate = true;
        #endregion

        private bool mWelcomeScreenShown;

        public string CabinLayoutsReadoutPath
        {
            get => mCabinLayoutsReadoutPath;
            set
            {
                mCabinLayoutsReadoutPath = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(ReadoutPathValid));
                InvokePropertyChanged(nameof(PathsValid));
            }
        }

        [JsonIgnore]
        public bool ReadoutPathValid => Directory.Exists(CabinLayoutsReadoutPath);

        public string CabinLayoutsEditPath
        {
            get => mCabinLayoutsEditPath;
            set
            {
                mCabinLayoutsEditPath = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(EditPathValid));
                InvokePropertyChanged(nameof(PathsValid));
            }
        }

        [JsonIgnore]
        public bool EditPathValid => Directory.Exists(CabinLayoutsEditPath);

        [JsonIgnore]
        public bool PathsValid => ReadoutPathValid && EditPathValid;

        public bool ShowWarningWhenIssuesPresent
        {
            get => mShowWarningWhenIssuesPresent;
            set
            {
                mShowWarningWhenIssuesPresent = value;
                InvokePropertyChanged();
            }
        }

        public bool WelcomeScreenShown
        {
            get => mWelcomeScreenShown;
            set
            {
                mWelcomeScreenShown = value;
                InvokePropertyChanged();
            }
        }

        #region Editor toggles
        public bool OpenFolderWithEditedLayout
        {
            get => mOpenFolderWithEditedLayout;
            set
            {
                mOpenFolderWithEditedLayout = value;
                InvokePropertyChanged();
            }
        }

        public bool OpenSLCTargetFolder
        {
            get => mOpenSLCTargetFolder;
            set
            {
                mOpenSLCTargetFolder = value;
                InvokePropertyChanged();
            }
        }

        public bool CopyLayoutsAfterSave
        {
            get => mCopyLayoutsAfterSave;
            set
            {
                mCopyLayoutsAfterSave = value;
                InvokePropertyChanged();
            }
        }
        #endregion

        #region Utility toggles
        public bool OpenFoldersAfterSaving
        {
            get => mOpenFoldersAfterSaving;
            set
            {
                mOpenFoldersAfterSaving = value;
                InvokePropertyChanged();
            }
        }

        public bool RunCommandPromptHidden
        {
            get => mRunCommandPromptHidden;
            set
            {
                mRunCommandPromptHidden = value;
                InvokePropertyChanged();
            }
        }

        public bool OnlyPromptOnceForPrivileges
        {
            get => mOnlyPromptOnceForPrivileges;
            set
            {
                mOnlyPromptOnceForPrivileges = value;
                InvokePropertyChanged();
            }
        }
        #endregion

        #region Updater settings
        public bool AutoSearchForUpdates
        {
            get => mAutoSearchForUpdates;
            set
            {
                mAutoSearchForUpdates = value;
                InvokePropertyChanged();
            }
        }

        public bool ShowChangesAfterUpdate
        {
            get => mShowChangesAfterUpdate;
            set
            {
                mShowChangesAfterUpdate = value;
                InvokePropertyChanged();
            }
        }

        public int LastVersionChangelogShown { get; set; }
        #endregion

        [JsonConstructor]
        public AppSettings(string cabinLayoutsReadoutPath, string cabinLayoutsEditPath, bool welcomeScreenShown,
            bool showWarningWhenIssuesPresent, bool openFoldersAfterSaving, bool openFolderWithEditedLayout, bool openSLCTargetFolder,
            bool copyLayoutsAfterSave, bool runCommandPromptHidden, bool onlyPromptOnceForPrivileges,
            bool autoSearchForUpdates, bool showChangesAfterUpdate, int lastVersionChangelogShown) : this()
        {
            mCabinLayoutsReadoutPath = cabinLayoutsReadoutPath;
            mCabinLayoutsEditPath = cabinLayoutsEditPath;
            mWelcomeScreenShown = welcomeScreenShown;
            mShowWarningWhenIssuesPresent = showWarningWhenIssuesPresent;

            mOpenFoldersAfterSaving = openFoldersAfterSaving;
            mOpenFolderWithEditedLayout = openFolderWithEditedLayout;
            mOpenSLCTargetFolder = openSLCTargetFolder;

            mCopyLayoutsAfterSave = copyLayoutsAfterSave;
            mRunCommandPromptHidden = runCommandPromptHidden;
            mOnlyPromptOnceForPrivileges = onlyPromptOnceForPrivileges;

            mAutoSearchForUpdates = autoSearchForUpdates;
            mShowChangesAfterUpdate = showChangesAfterUpdate;
            LastVersionChangelogShown = lastVersionChangelogShown;

            if (lastVersionChangelogShown <= 0)
            {
                mShowWarningWhenIssuesPresent = true;
                mOpenFoldersAfterSaving = true;
                mOpenFolderWithEditedLayout = true;
                mOpenSLCTargetFolder = true;

                mOnlyPromptOnceForPrivileges = true;

                mAutoSearchForUpdates = true;
                mShowChangesAfterUpdate = true;
            }
        }

        public AppSettings() : base(false)
        {
            mCabinLayoutsReadoutPath = Path.Combine(App.DefaultSLCLayoutsPath, "CabinLayouts");

            mCabinLayoutsEditPath = Path.Combine(App.DefaultEditorLayoutsPath, "CabinLayouts");
        }

        /// <summary>
        /// Loads existing <see cref="AppSettings"/> from a json file.
        /// </summary>
        /// <param name="fi">A <see cref="FileInfo"/> object containing the path to the app settings</param>
        public static AppSettings Load(FileInfo fi)
        {
            AppSettings appSettings = LoadFile(fi);
            appSettings.filePath = fi.Directory.FullName;
            appSettings.fromFile = true;
            return appSettings;
        }

        public void Save(string parentPath = null)
        {
            if (string.IsNullOrWhiteSpace(parentPath))
            {
                throw new Exception("ParentPath needs to have a value if AppSettings file is being created!");
            }

            fileName = "settings.json";
            SaveFile(parentPath, this);
        }
    }
}
