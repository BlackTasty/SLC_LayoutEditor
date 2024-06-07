using Microsoft.Win32;
using Newtonsoft.Json.Bson;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Guide;
using SLC_LayoutEditor.Core.Patcher;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IUIManager
    {
        private Uri currentTheme;
        private static DateTime? suspendStart;

        private static readonly string oldDefaultEditorLayoutsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "SLC Layout Editor");
        private static readonly string defaultEditorLayoutsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "Tasty Apps", "SLC Layout Editor");

        private static readonly string tempPath = Path.Combine(Path.GetTempPath(), "Tasty Apps", "SLC Layout Editor");
        private static readonly string thumbnailsPath = Path.Combine(tempPath, "thumbnails");
        private static readonly string snapshotsPath = Path.Combine(tempPath, "snapshots");

        private static UpdateManager patcher;

        public static DateTime SessionStart { get; set; }

        public static bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public static string DefaultEditorLayoutsPath => defaultEditorLayoutsPath;

        public static string ThumbnailsPath => thumbnailsPath;

        public static string SnapshotsPath => snapshotsPath;

        public static string TempPath => tempPath;

        public static AppSettings Settings { get; set; } = new AppSettings();

        public static bool IsDialogOpen { get;set; }

        internal static UpdateManager Patcher => patcher;

        public static bool IsStartup { get; set; } = true;

        internal static GuidedTour GuidedTour { get; set; }

        internal static bool IsKeybindSheetOpen { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
#if DEBUG
            RunApp(args);
#else
            try
            {
                RunApp(args);
            }
            catch (Exception ex)
            {
                RecordUsageTime(true);
                Logger.Default.WriteLog("SLC Layout Editor crashed with a fatal exception! Version {0}", LogType.FATAL, ex,
                    PatcherUtil.SerializeVersionNumber(Assembly.GetExecutingAssembly().GetName().Version.ToString(), 3));

                Mediator.Instance.NotifyColleagues(ViewModelMessage.CreateSnapshot);

                MessageBox.Show(
                    string.Format("It seems like the editor ({0}) has crashed!\n\n" +
                        "Please send the log file via Discord to midnightbagel, this way I can look into what happened.", 
                        GetVersionText()), 
                    "This is awkward but...", MessageBoxButton.OK);
            }
#endif
        }

        public void RefreshTheme()
        {
            if (App.Settings.EnableSeasonalThemes)
            {
                DateTime now = DateTime.Now;
                string targetTheme = null;
                switch (now.Month)
                {
                    case 6: // Apply pride theme
                        targetTheme = "Pride";
                        break;
                    case 12: // Apply christmas theme
                        if (now.Day <= 25)
                        {
                            targetTheme = "Christmas";
                        }
                        break;
                }

                if (targetTheme != null)
                {
                    ToggleTheme(new Uri(FixedValues.URI_THEMES + targetTheme + "Theme.xaml"));
                }

                if (currentTheme == null)
                {
                    ToggleTheme(null);
                }
            }
            else
            {
                currentTheme = null;
                ToggleTheme(null);
            }
        }

        public static string GetTemplatePath(string aircraftName)
        {
            return Path.Combine(Settings.CabinLayoutsEditPath, aircraftName, "templates");
        }

        public static void SaveAppSettings()
        {
            Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
        }

        internal static string GetVersionText(bool versionNumberOnly = false)
        {
            string version = PatcherUtil.SerializeVersionNumber(Assembly.GetExecutingAssembly().GetName().Version.ToString(), 3);

            if (version.EndsWith(".0"))
            {
                version = version.Substring(0, version.Length - 2);
            }

            if (!versionNumberOnly)
            {
                string versionText = string.Format("v{0}", version);

#if DEBUG
                versionText += "dev";
#endif

                return versionText;
            }
            else
            {
                return version;
            }
        }

        internal static void InitPatcher()
        {
            if (patcher == null)
            {
                patcher = new UpdateManager();
            }
        }

        private static void RunApp(string[] args)
        {
            Logger.Default.WriteLog("Preparing SLC Layout Editor v{0}...", GetVersionText(true));
            FileInfo fi = new FileInfo("settings.json");

            if (!args.Contains("-clean"))
            {
                if (fi.Exists)
                {
                    Settings = AppSettings.Load(fi);
                }
                else
                {
                    Logger.Default.WriteLog("No config file exists, creating file \"{0}\"", fi.Name);
                    Settings = new AppSettings();
                    Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
                }
            }
            else
            {
                Logger.Default.WriteLog("-clean flag has been set as startup parameter, resetting application...");
                if (fi.Exists)
                {
                    Logger.Default.WriteLog("Removing edited cabin layouts...");
                    Directory.Delete(Settings.CabinLayoutsEditPath, true);

                    Logger.Default.WriteLog("Resetting config file...");
                    fi.Delete();
                    Settings = new AppSettings();
                    Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
                }
            }

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            RunMigrations();
            Directory.CreateDirectory(Settings.CabinLayoutsEditPath);
            CheckTemplates();
            InitPatcher();

            Logger.Default.WriteLog("Preparations complete, starting up editor...");
            App app = new App()
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            app.InitializeComponent();
            app.Run();

            Logger.Default.WriteLog("Editor shutting down...");
            RecordUsageTime(false);
            SaveAppSettings();

            /*if (Directory.Exists(SnapshotsPath))
            {
                Directory.Delete(SnapshotsPath, true);
                Logger.Default.WriteLog("Removed leftover cabin layout mSnapshots");
            }*/


            // Installation of an update pending, restart editor
            if (App.Patcher.RestartAfterClose)
            {
                Logger.Default.WriteLog("Update installed, user requested app restart...");
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
            }
        }

        private static void RecordUsageTime(bool saveSettings)
        {
            if (Settings == null) return;

            DateTime now = DateTime.Now;
            TimeSpan sessionTime = now - SessionStart;

            if (suspendStart.HasValue)
            {
                TimeSpan suspendTime = now - suspendStart.Value;
                sessionTime -= suspendTime;
            }

            Settings.UsageTime += sessionTime;
            if (saveSettings)
            {
                Settings.Save();
            }
        }

        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    suspendStart = DateTime.Now;
                    break;
                case PowerModes.Resume:
                    RecordUsageTime(false);
                    SessionStart = DateTime.Now;
                    suspendStart = null;
                    break;
            }
        }

        private static void RunMigrations()
        {
#region Migrate layout folder from default path to new path
            if (!Directory.Exists(defaultEditorLayoutsPath) && Directory.Exists(oldDefaultEditorLayoutsPath))
            {
                Logger.Default.WriteLog("Migrating directory for layouts edited with the editor...");
                if (Directory.Exists(defaultEditorLayoutsPath))
                {
                    Directory.Delete(defaultEditorLayoutsPath, true);
                }
                Directory.CreateDirectory(defaultEditorLayoutsPath);
                Directory.Move(oldDefaultEditorLayoutsPath, defaultEditorLayoutsPath);

                Logger.Default.WriteLog("Layout editor directory migrated!");
            }

            if (Settings.CabinLayoutsEditPath == oldDefaultEditorLayoutsPath)
            {
                Logger.Default.WriteLog("Adjusting {0} property in app settings...", nameof(Settings.CabinLayoutsEditPath));
                Settings.CabinLayoutsEditPath = defaultEditorLayoutsPath;
            }
#endregion
        }

        public static void CopyTemplates()
        {
            Logger.Default.WriteLog("Copying baked-in templates to cabin layouts directory...");
            int copiedTemplates = 0;

            foreach (string bakedTemplatePath in Util.GetBakedTemplates())
            {
                string template = Util.ReadTextResource(bakedTemplatePath);

                int newLineIndex = template.IndexOf("\r\n");
                string aircraftName = template.Substring(0, newLineIndex);
                template = template.Substring(newLineIndex + 2);

                string templatePath = GetTemplatePath(aircraftName);
                string templateFilePath = Path.Combine(templatePath, "Default.txt");

                if (!File.Exists(templateFilePath))
                {
                    Directory.CreateDirectory(templatePath);
                    File.WriteAllText(templateFilePath, template);
                    copiedTemplates++;
                }
                else
                {
                    Logger.Default.WriteLog("Skipped template for aircraft \"{0}\" as it exists already.", aircraftName);
                }
            }

            Logger.Default.WriteLog("Copying complete! {0} templates have been created.", copiedTemplates);
            Settings.TemplatesCopied = true;
            SaveAppSettings();
        }

        private static void CheckTemplates()
        {
            if (!Settings.TemplatesCopied)
            {
                CopyTemplates();
            }
        }

        private void ToggleTheme(Uri themeUri)
        {
            if (themeUri != null && currentTheme == null && !Resources.MergedDictionaries.Any(x => x.Source.OriginalString == themeUri.OriginalString))
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary()
                {
                    Source = themeUri
                });

                currentTheme = themeUri;
            }
            else if (currentTheme != null && 
                Resources.MergedDictionaries.FirstOrDefault(x => x.Source.OriginalString == currentTheme.OriginalString) is ResourceDictionary theme)
            {
                Resources.MergedDictionaries.Remove(theme);
            }
        }
    }
}
