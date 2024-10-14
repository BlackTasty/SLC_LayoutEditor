using System.Collections.Generic;
using System.IO;

namespace SLC_LayoutEditor.Core.Patcher
{
    static class PatcherUtil
    {
        public static List<string> BackupDirectory(string sourcePath, string destPath)
        {
            return BackupFiles(new DirectoryInfo(sourcePath), destPath);

            //Directory.Move(destPath, destPath + ".BAK");
            //Directory.Move(sourcePath, destPath);
        }

        public static List<string> BackupFiles(DirectoryInfo di, string destPath)
        {
            List<string> paths = new List<string>();
            foreach (FileInfo fi in di.GetFiles())
            {
                paths.Add(BackupFile(fi.FullName, destPath + "\\" + fi.Name));
            }

            return paths;
        }

        public static string BackupFile(string sourcePath, string destPath, string[] whitelist)
        {
            if (!IsWhitelistFile(sourcePath, whitelist))
            {
                BackupFile(sourcePath, destPath);
            }
            return destPath + ".BAK";
        }

        public static string BackupFile(string sourcePath, string destPath)
        {
            if (File.Exists(destPath))
            {
                Util.SafeDeleteFile(destPath + ".BAK");
                File.Move(destPath, destPath + ".BAK");
            }
            File.Move(sourcePath, destPath);

            return destPath + ".BAK";
        }

        public static bool IsWhitelistFile(string path, string[] whitelist)
        {
            if (whitelist != null)
            {
                foreach (string str in whitelist)
                {
                    if (path.Contains(str))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Serializes the input and returns a valid version number (e.g.: 3.2 = 3.2.0.0)
        /// </summary>
        /// <param name="version">Raw version string</param>
        /// <param name="subVersionCount">The number of subversions (3 is default)</param>
        /// <returns></returns>
        public static string SerializeVersionNumber(string version, int subVersionCount)
        {
            for (int i = version.Split('.').Length; i <= subVersionCount; i++)
                version += ".0";

            return version;
        }

        /// <summary>
        /// Returns a version string as integer
        /// </summary>
        /// <param name="version">Raw version string</param>
        /// <param name="subVersionCount">The number of subversions (3 is default). Can also be 0!</param>
        /// <returns></returns>
        internal static int ParseVersion(string version, int subVersionCount)
        {
            if (subVersionCount > 0)
                return int.Parse(SerializeVersionNumber(version, subVersionCount).Replace(new string[] { ".", " Full" }));
            else
                return int.Parse(version.Replace(new string[] { ".", " Full" }));
        }
    }
}
