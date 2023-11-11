using SLC_LayoutEditor.Core;
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
        private static readonly string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SLC Layout Editor");

        public static bool IsDesignMode => DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public static AppSettings Settings { get; set; } = new AppSettings();

        [STAThread]
        public static void Main(string[] args)
        {
            try
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

                Directory.CreateDirectory(Settings.CabinLayoutsEditPath);

                //LoadAppSettings();

                App app = new App()
                {
                    ShutdownMode = ShutdownMode.OnMainWindowClose
                };
                app.InitializeComponent();
                app.Run();

                SaveAppSettings();
            }
            catch (Exception ex)
            {
                Logger.Default.WriteLog("SLC Layout Editor crashed with a fatal exception! Version {0}", LogType.FATAL, ex,
                    PatcherUtil.SerializeVersionNumber(Assembly.GetExecutingAssembly().GetName().Version.ToString(), 3));
            }
        }

        public static void SaveAppSettings()
        {
            Settings.Save(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
