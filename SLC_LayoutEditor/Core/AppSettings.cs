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
        private string mCabinLayoutsEditPath;

        #region Editor toggles
        private bool mShowWarningWhenIssuesPresent = true;

        private bool mOpenFolderWithEditedLayout = true;
        #endregion

        #region Updater settings
        private bool mAutoSearchForUpdates = true;
        private bool mShowChangesAfterUpdate = true;
        #endregion

        private bool mWelcomeScreenShown;
        private bool mTemplatesCopied;

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
        public bool PathsValid => EditPathValid;

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

        public bool TemplatesCopied { get; set; }

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
        public AppSettings(string cabinLayoutsEditPath, bool welcomeScreenShown, bool templatesCopied,
            bool showWarningWhenIssuesPresent, bool openFolderWithEditedLayout,
            bool autoSearchForUpdates, bool showChangesAfterUpdate, int lastVersionChangelogShown) : this()
        {
            mCabinLayoutsEditPath = cabinLayoutsEditPath;
            mWelcomeScreenShown = welcomeScreenShown;
            TemplatesCopied = templatesCopied;
            mShowWarningWhenIssuesPresent = showWarningWhenIssuesPresent;

            mOpenFolderWithEditedLayout = openFolderWithEditedLayout;

            mAutoSearchForUpdates = autoSearchForUpdates;
            mShowChangesAfterUpdate = showChangesAfterUpdate;
            LastVersionChangelogShown = lastVersionChangelogShown;

            if (lastVersionChangelogShown <= 0)
            {
                mShowWarningWhenIssuesPresent = true;
                mOpenFolderWithEditedLayout = true;

                mAutoSearchForUpdates = true;
                mShowChangesAfterUpdate = true;
            }
        }

        public AppSettings() : base(false)
        {
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
