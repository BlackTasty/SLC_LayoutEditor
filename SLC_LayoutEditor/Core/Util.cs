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

        public static bool HasLayoutChanged(CabinLayout layout)
        {
            return layout != null ? CompareLayoutHashes(layout.FilePath, layout.ToLayoutFile()) : false;
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
