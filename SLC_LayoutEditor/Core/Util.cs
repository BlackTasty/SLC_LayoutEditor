using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.IO.Pipes;
using Tasty.Logging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Reflection;
using SLC_LayoutEditor.Core.Cabin;
using System.Windows.Media.Imaging;

namespace SLC_LayoutEditor.Core
{
    static class Util
    {
        public static int GetProblemCount(int existingProblemCount, params bool[] valuesToCheck)
        {
            int problemCount = existingProblemCount;
            for (int i = 0; i < valuesToCheck.Length; i++)
            {
                if (!valuesToCheck[i])
                {
                    problemCount++;
                }
            }

            return problemCount;
        }

        public static void OpenFolder(string path, bool selectTarget = true)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = string.Format("/e{0}, \"{1}\"", selectTarget ? ", /select" : "", path)
            });
        }

        public static bool IsShiftDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) ||
                   Keyboard.IsKeyDown(Key.RightShift);
        }

        public static bool IsControlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) ||
                   Keyboard.IsKeyDown(Key.RightCtrl);
        }

        public static string GetFloorName(int floor)
        {
            switch (floor)
            {
                case 1:
                    return "Lower deck";
                case 2:
                    return "Upper deck";
                default:
                    string suffix;
                    switch (int.Parse(floor.ToString().LastOrDefault().ToString()))
                    {
                        case 1:
                            suffix = "st";
                            break;
                        case 2:
                            suffix = "nd";
                            break;
                        case 3:
                            suffix = "rd";
                            break;
                        default:
                            suffix = "th";
                            break;
                    }

                    return string.Format("{0}{1} deck", floor, suffix);
            }
        }

        internal static BitmapImage LoadImage(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                try
                {
                    BitmapImage bmp = new BitmapImage();

                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = fs;
                        bmp.EndInit();
                    }

                    return bmp;
                }
                catch (Exception ex)
                {
                    Logger.Default.WriteLog("There was an error loading an image!", ex);
                    return null;
                }
            }
            else return null;
        }

        public static string SelectFolder(string title, string currentFolder, bool showNewFolderButton)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog()
            {
                Description = title,
                ShowNewFolderButton = showNewFolderButton,
                SelectedPath = currentFolder
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
        }

        public static IEnumerable<string> GetBakedTemplates()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            string folderName = string.Format("{0}.Resources.LayoutTemplates", executingAssembly.GetName().Name);
            return executingAssembly
                .GetManifestResourceNames()
                .Where(r => r.StartsWith(folderName) && r.EndsWith(".txt"));
                //.Select(r => r.Substring(folderName.Length + 1));
        }

        public static bool HasLayoutChanged(CabinLayout layout)
        {
            return layout != null && (!File.Exists(layout.FilePath) || CompareLayoutHashes(layout.FilePath, layout.ToLayoutFile()));
        }

        public static bool SafeDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    File.Delete(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Default.WriteLog("Unable to delete file {0}!", ex, LogType.WARNING, new FileInfo(filePath).Name);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static string ReadTextResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static bool CompareLayoutHashes(string originalFilePath, string current)
        {
            if (!File.Exists(originalFilePath))
            {
                return false;
            }
            return !GetSHA256Hash(File.ReadAllText(originalFilePath).ToUpper()).Equals(GetSHA256Hash(current));
        }

        private static string GetSHA256Hash(string value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return string.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }
    }
}
