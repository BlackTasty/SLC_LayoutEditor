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
        private bool mWelcomeScreenShown;

        public string CabinLayoutsReadoutPath
        {
            get => mCabinLayoutsReadoutPath;
            set
            {
                mCabinLayoutsReadoutPath = value;
                InvokePropertyChanged();
                InvokePropertyChanged("PathsValid");
            }
        }

        public string CabinLayoutsEditPath
        {
            get => mCabinLayoutsEditPath;
            set
            {
                mCabinLayoutsEditPath = value;
                InvokePropertyChanged();
                InvokePropertyChanged("PathsValid");
            }
        }

        public bool PathsValid => Directory.Exists(CabinLayoutsReadoutPath) && Directory.Exists(CabinLayoutsEditPath);

        public bool WelcomeScreenShown
        {
            get => mWelcomeScreenShown;
            set
            {
                mWelcomeScreenShown = value;
                InvokePropertyChanged();
            }
        }

        [JsonConstructor]
        public AppSettings(string cabinLayoutsReadoutPath, string cabinLayoutsEditPath, bool welcomeScreenShown) : this()
        {
            mCabinLayoutsReadoutPath = cabinLayoutsReadoutPath;
            mCabinLayoutsEditPath = cabinLayoutsEditPath;
            mWelcomeScreenShown = welcomeScreenShown;
        }

        public AppSettings() : base(false)
        {
            mCabinLayoutsReadoutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 
                "Lanilogic", "Self Loading Cargo", "CabinLayouts");

            mCabinLayoutsEditPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SLC Layout Editor", "CabinLayouts");
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
