﻿using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Patcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Tasty.Logging;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string oldDefaultEditorLayoutsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "SLC Layout Editor");
        private static readonly string defaultEditorLayoutsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "Tasty Apps", "SLC Layout Editor");

        private static readonly string tempPath = Path.Combine(Path.GetTempPath(), "Tasty Apps", "SLC Layout Editor");
        private static readonly string thumbnailsPath = Path.Combine(tempPath, "thumbnails");

        public static bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public static string DefaultEditorLayoutsPath => defaultEditorLayoutsPath;

        public static string ThumbnailsPath => thumbnailsPath;

        public static string TempPath => tempPath;

        public static AppSettings Settings { get; set; } = new AppSettings();

        public static bool IsDialogOpen { get;set; }

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
                Logger.Default.WriteLog("SLC Layout Editor crashed with a fatal exception! Version {0}", LogType.FATAL, ex,
                    PatcherUtil.SerializeVersionNumber(Assembly.GetExecutingAssembly().GetName().Version.ToString(), 3));
            }
#endif
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
                string versionText = string.Format(" v{0}", version);

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

        private static void RunApp(string[] args)
        {
            FileInfo fi = new FileInfo("settings.json");

            if (fi.Exists)
            {
                Settings = AppSettings.Load(fi);
            }
            else
            {
                Settings = new AppSettings();
                Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
            }

            if (args.Contains("-clean"))
            {
                if (fi.Exists)
                {
                    Directory.Delete(Settings.CabinLayoutsEditPath, true);

                    fi.Delete();
                    Settings = new AppSettings();
                    Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
                }
            }

            RunMigrations();
            Directory.CreateDirectory(Settings.CabinLayoutsEditPath);
            CheckTemplates();

            //LoadAppSettings();

            App app = new App()
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            app.InitializeComponent();
            app.Run();

            SaveAppSettings();
        }

        private static void RunMigrations()
        {
#region Migrate layout folder from default path to new path
            if (Directory.Exists(oldDefaultEditorLayoutsPath))
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

        private static void CheckTemplates()
        {
            if (!Settings.TemplatesCopied)
            {
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
                    }
                }

                Settings.TemplatesCopied = true;
                SaveAppSettings();
            }
        }
    }
}
