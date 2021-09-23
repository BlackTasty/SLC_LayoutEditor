using SLC_LayoutEditor.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SLC Layout Editor");

        public static AppSettings Settings { get; set; } = new AppSettings();

        [STAThread]
        public static void Main(string[] args)
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
                    Directory.Delete(App.Settings.CabinLayoutsEditPath, true);

                    fi.Delete();
                    Settings = new AppSettings();
                    Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
                }
            }

            Directory.CreateDirectory(App.Settings.CabinLayoutsEditPath);

            //LoadAppSettings();

            App app = new App()
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            app.InitializeComponent();
            app.Run();

            SaveAppSettings();
        }

        public static void LoadAppSettings()
        {
            FileInfo fi = new FileInfo(Path.Combine(basePath, "settings.json"));

            if (fi.Exists)
            {
                Settings = AppSettings.Load(fi);
            }
        }

        public static void SaveAppSettings()
        {
            Settings.Save(basePath);
        }
    }
}
