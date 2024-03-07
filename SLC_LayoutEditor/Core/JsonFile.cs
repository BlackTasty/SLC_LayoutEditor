using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Observer;
using Tasty.ViewModel;
using Tasty.Logging;

namespace SLC_LayoutEditor.Core
{
    public class JsonFile<T> : ViewModelBase
    {
        protected string filePath;
        protected string fileName;
        protected bool fromFile;
        protected bool isFile;
        protected ObserverManager observerManager = new ObserverManager();

        [JsonIgnore]
        public ObserverManager ObserverManager => observerManager;

        [JsonIgnore]
        public bool UnsavedChanges
        {
            get => observerManager.UnsavedChanges;
        }

        /// <summary>
        /// Path excluding file name
        /// </summary>
        [JsonIgnore]
        public string FilePath => filePath;

        [JsonIgnore]
        public string FileName => fileName;

        [JsonIgnore]
        public bool FromFile => fromFile;

        public JsonFile(bool observersEnabled)
        {
            observerManager.IsEnabled = observersEnabled;
        }

        public JsonFile(FileInfo fi, bool observersEnabled) : this(observersEnabled)
        {
            filePath = fi.DirectoryName;
            fileName = fi.Name;
            fi.Refresh();

            fromFile = true;
            isFile = true;
        }

        public JsonFile(DirectoryInfo di, bool observersEnabled) : this(observersEnabled)
        {
            filePath = di.Parent.FullName;
            fileName = di.Name;
            di.Refresh();

            fromFile = true;
        }

        public virtual string GetFullPath()
        {
            return Path.Combine(filePath, fileName);
        }

        public virtual void Delete()
        {
            Delete(false);
        }

        public void Delete(bool forceDeleteParentDir = false)
        {
            // Return if object has not been saved to file yet
            if (!fromFile)
            {
                return;
            }

            if (isFile && !forceDeleteParentDir)
            {
                string path = Path.Combine(filePath, fileName);
                Util.SafeDeleteFile(path);
            }
            else
            {
                if (Directory.Exists(filePath))
                {
                    Directory.Delete(filePath, true);
                }
            }
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        protected virtual void SaveFile(T @object)
        {
            Logger.Default.WriteLog("Saving config file \"{0}\"...", new FileInfo(filePath).Name);
            string json = JsonConvert.SerializeObject(@object);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("FileContent must be set! Use SaveFile(string fileContent, T @object) to create a new file instead!");
            }

            File.WriteAllText(Path.Combine(filePath, fileName), json);
            fromFile = true;
            FileAttributes attr = File.GetAttributes(Path.Combine(filePath, fileName));

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                isFile = true;
            }
            observerManager.ResetObservers();
            observerManager.ChangeObservers.FirstOrDefault(x => x.PropertyName == "")?.Reset();
        }

        protected virtual void SaveFile(string filePath, T @object)
        {
            /*if (!Directory.Exists(fileContent))
            {
                Directory.CreateDirectory(fileContent);
            }*/

            this.filePath = filePath;
            SaveFile(@object);
        }

        protected virtual T LoadFile()
        {
            string targetPath = Path.Combine(filePath, fileName);
            if (!fromFile || !File.Exists(targetPath))
            {
                return default;
            }

            string json = File.ReadAllText(targetPath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        protected static T LoadFile(FileInfo fi)
        {
            Logger.Default.WriteLog("Loading config file \"{0}\"...", fi.Name);
            string json = File.ReadAllText(fi.FullName);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
