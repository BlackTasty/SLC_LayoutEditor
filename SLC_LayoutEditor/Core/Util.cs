using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;

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

        public static void OpenFolderAndSelect(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = string.Format("/e, /select, \"{0}\"", path)
            });
        }
        public static bool IsShiftDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) ||
                   Keyboard.IsKeyDown(Key.RightShift);
        }
    }
}
