using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace SLC_LayoutEditor.Core
{
    public class AppSettings : JsonFile<AppSettings>
    {
        private string mCabinLayoutsEditPath;

        #region Editor toggles
        private bool mShowWarningWhenIssuesPresent = true;

        private bool mOpenLayoutAfterSaving;
        private bool mCopyLayoutCodeToClipboard = true;
        private bool mNavigateToSLCWebsite = true;
        #endregion

        #region Updater settings
        private bool mAutoSearchForUpdates = true;
        private bool mShowChangesAfterUpdate = true;
        #endregion

        private bool mWelcomeScreenShown;
        private bool mHideSidebarAfterLoadingLayout;

        #region Remember last layout settings
        private bool mRememberLastLayout;
        private string mLastLayoutSet;
        private string mLastLayout;
        private bool mLastLayoutWasTemplate;
        #endregion

        private bool mEnableSeasonalThemes = true;

        private bool mGettingStartedGuideShown;

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

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
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

        public bool HideSidebarAfterLoadingLayout
        {
            get => mHideSidebarAfterLoadingLayout;
            set
            {
                mHideSidebarAfterLoadingLayout = value;
                InvokePropertyChanged();
            }
        }

        #region Editor toggles
        public bool OpenLayoutAfterSaving
        {
            get => mOpenLayoutAfterSaving;
            set
            {
                mOpenLayoutAfterSaving = value;
                InvokePropertyChanged();
            }
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool CopyLayoutCodeToClipboard
        {
            get => mCopyLayoutCodeToClipboard;
            set
            {
                mCopyLayoutCodeToClipboard = value;
                InvokePropertyChanged();
            }
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool NavigateToSLCWebsite
        {
            get => mNavigateToSLCWebsite;
            set
            {
                mNavigateToSLCWebsite = value;
                InvokePropertyChanged();
            }
        }
        #endregion

        #region Updater settings
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool AutoSearchForUpdates
        {
            get => mAutoSearchForUpdates;
            set
            {
                mAutoSearchForUpdates = value;
                InvokePropertyChanged();
            }
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
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

        #region Remember last layout settings
        public bool RememberLastLayout
        {
            get => mRememberLastLayout;
            set
            {
                mRememberLastLayout = value;
                InvokePropertyChanged();
            }
        }

        public string LastLayoutSet
        {
            get => mLastLayoutSet;
            set
            {
                mLastLayoutSet = value;
                InvokePropertyChanged();
            }
        }

        public string LastLayout
        {
            get => mLastLayout;
            set
            {
                mLastLayout = value;
                InvokePropertyChanged();
            }
        }

        public bool LastLayoutWasTemplate
        {
            get => mLastLayoutWasTemplate;
            set
            {
                mLastLayoutWasTemplate = value;
                InvokePropertyChanged();
            }
        }
        #endregion

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool EnableSeasonalThemes
        {
            get => mEnableSeasonalThemes;
            set
            {
                mEnableSeasonalThemes = value;
                InvokePropertyChanged();
            }
        }

        public bool GettingStartedGuideShown
        {
            get => mGettingStartedGuideShown;
            set
            {
                mGettingStartedGuideShown = value;
                InvokePropertyChanged();
            }
        }

        [JsonConstructor]
        public AppSettings(string cabinLayoutsEditPath, bool welcomeScreenShown, bool templatesCopied,
            bool showWarningWhenIssuesPresent, bool openLayoutAfterSaving, bool copyLayoutCodeToClipboard, bool navigateToSLCWebsite,
            bool autoSearchForUpdates, bool showChangesAfterUpdate, int lastVersionChangelogShown, 
            bool hideSidebarAfterLoadingLayout, bool rememberLastLayout, string lastLayoutSet, string lastLayout, bool lastLayoutWasTemplate, 
            bool enableSeasonalThemes, bool gettingStartedGuideShown) : this()
        {
            mCabinLayoutsEditPath = cabinLayoutsEditPath;
            mWelcomeScreenShown = welcomeScreenShown;
            TemplatesCopied = templatesCopied;
            mShowWarningWhenIssuesPresent = showWarningWhenIssuesPresent;

            mOpenLayoutAfterSaving = openLayoutAfterSaving;
            mCopyLayoutCodeToClipboard = copyLayoutCodeToClipboard;
            mNavigateToSLCWebsite = navigateToSLCWebsite;

            mAutoSearchForUpdates = autoSearchForUpdates;
            mShowChangesAfterUpdate = showChangesAfterUpdate;
            LastVersionChangelogShown = lastVersionChangelogShown;

            if (lastVersionChangelogShown <= 0)
            {
                mShowWarningWhenIssuesPresent = true;
                mOpenLayoutAfterSaving = true;

                mAutoSearchForUpdates = true;
                mShowChangesAfterUpdate = true;
            }
            mHideSidebarAfterLoadingLayout = hideSidebarAfterLoadingLayout;
            mRememberLastLayout = rememberLastLayout;
            mLastLayoutSet = lastLayoutSet;
            mLastLayout = lastLayout;
            mLastLayoutWasTemplate = lastLayoutWasTemplate;
            mEnableSeasonalThemes = enableSeasonalThemes;
            mGettingStartedGuideShown = gettingStartedGuideShown;
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
                throw new Exception("ParentPath needs to have a data if AppSettings file is being created!");
            }

            fileName = "settings.json";
            SaveFile(parentPath, this);
        }
    }
}
