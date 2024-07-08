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
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.UI.Dialogs;
using System.Windows.Controls;

namespace SLC_LayoutEditor.Core
{
    static class Util
    {
        private static string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

        public static void OpenFile(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = string.Format("\"{0}\"", path)
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

        public static void MoveThumbnailToDelete(CabinDeck cabinDeck)
        {
            string thumbnailPath = Path.Combine(cabinDeck.ThumbnailDirectory, cabinDeck.ThumbnailFileName);
            MoveFileToDelete(new FileInfo(thumbnailPath));
        }

        public static void MoveFileToDelete(FileInfo fi)
        {
            string sourcePath = fi.FullName;
            string destPath = Path.Combine(App.TempPath, "deleted", string.Format(".{0}", fi.Name));

            MoveFileToDelete(sourcePath, destPath);
        }

        public static void MoveFileToDelete(string sourcePath, string destPath)
        {
            string deleteDirectoryPath = new FileInfo(destPath).Directory.FullName;
            Directory.CreateDirectory(deleteDirectoryPath);

            File.Move(sourcePath, destPath);
            SafeDeleteFile(destPath);
            Directory.Delete(deleteDirectoryPath, true);
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
            string savedLayoutCode = File.ReadAllText(originalFilePath).ToUpper();
            if (savedLayoutCode.Trim() == "@")
            {
                savedLayoutCode = "";
            }
            return !GetSHA256Hash(savedLayoutCode).Equals(GetSHA256Hash(current));
        }

        public static string GetSHA256Hash(string value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return string.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }

        public static void RefreshTheme(System.Windows.Application app)
        {
            ((IUIManager)app).RefreshTheme();
        }

        public static Point GetChildCenterPosition(Rect parent, Rect child, bool centerHorizontally,
            bool centerVertically)
        {
            double centerX = parent.X + parent.Width / 2 - child.Width / 2;
            double centerY = parent.Y + parent.Height / 2 - child.Height / 2;

            return new Point(centerHorizontally ? centerX : child.X,
                centerVertically ? centerY : child.Y);
        }

        public static Brush GetBackgroundFromResources(string name)
        {
            return (Brush)App.Current.FindResource(name);
        }

        public static Pen GetBorderColorFromResources(string name)
        {
            return GetBorderColorFromResources(name, FixedValues.DEFAULT_BORDER_THICKNESS);
        }

        public static Pen GetBorderColorFromResources(string name, double borderThickness)
        {
            return new Pen(GetBackgroundFromResources(name), borderThickness);
        }

        public static string GetLettersForSeatNumeration(int rows, int startIndex = 0)
        {
            if (rows > letters.Length)
            {
                return null;
            }

            return letters.Substring(startIndex, rows);
        }

        public static IDialog BeginCreateCabinLayout(bool isTemplatingMode, CabinLayoutSet targetSet)
        {
            Logger.Default.WriteLog("User requested creating a new {0} for aircraft \"{1}\"...", isTemplatingMode ? "template" : "layout", targetSet.AircraftName);

            if (!isTemplatingMode)
            {
                return new CreateCabinLayoutDialog(targetSet.CabinLayouts.Select(x => x.LayoutName),
                    targetSet.GetTemplatePreviews(), isTemplatingMode);
            }
            else
            {
                return new CreateTemplateDialog(targetSet.Templates.Select(x => x.LayoutName));
            }
        }

        public static T GetTemplatedControlFromListBoxItem<T>(object source, string templateRootName)
        {
            if (source is ListBoxItem item)
            {
                ContentPresenter itemContentPresenter = FindVisualChild<ContentPresenter>(item);

                if (itemContentPresenter == null) {
                    return default;
                }

                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate itemDataTemplate = itemContentPresenter.ContentTemplate;
                return (T)itemDataTemplate.FindName(templateRootName, itemContentPresenter);
            }
            else
            {
                return default;
            }
        }

        private static ChildItem FindVisualChild<ChildItem>(DependencyObject obj)
            where ChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ChildItem)
                {
                    return (ChildItem)child;
                }
                else
                {
                    ChildItem childOfChild = FindVisualChild<ChildItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
