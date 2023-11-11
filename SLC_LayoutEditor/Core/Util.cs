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


        private static Process pipeClient = new Process();

        public static void CopyLayoutToSLC(string sourcePath, string destPath)
        {
            new Thread(() =>
            {
                Logger.Default.WriteLog("Initializing LayoutTransfer script... (keep running: {0})", App.Settings.OnlyPromptOnceForPrivileges);
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "LayoutTransfer.exe",
                    Arguments = string.Format("\"-sourcePath={0}\" \"-destPath={1}\"{2}", sourcePath, destPath, App.Settings.RunCommandPromptHidden ? " -silent" : ""),
                    WindowStyle = App.Settings.RunCommandPromptHidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    CreateNoWindow = App.Settings.RunCommandPromptHidden
                };

                if (!App.Settings.OnlyPromptOnceForPrivileges)
                {
                    Process.Start(startInfo);
                }
                else
                {
                    if (!IsLayoutTransferScriptRunning())
                    {
                        pipeClient.StartInfo = startInfo;
                    }

                    using (AnonymousPipeServerStream pipeServer = new AnonymousPipeServerStream(PipeDirection.Out,
                        HandleInheritability.Inheritable))
                    {
                        Logger.Default.WriteLog("[PIPE] CurrentTransmissionMode: {0}", pipeServer.TransmissionMode);

                        // Pass the client process a handle to the server.
                        if (!IsLayoutTransferScriptRunning())
                        {
                            pipeClient.StartInfo.Arguments = string.Format("{0} {1}",
                                pipeServer.GetClientHandleAsString(), pipeClient.StartInfo.Arguments);
                            pipeClient.StartInfo.UseShellExecute = false;
                            pipeClient.Start();

                            pipeServer.DisposeLocalCopyOfClientHandle();
                        }

                        try
                        {
                            // Read user input and send that to the client process.
                            using (StreamWriter sw = new StreamWriter(pipeServer))
                            {
                                sw.AutoFlush = true;
                                // Send a 'sync message' and wait for client to receive it.
                                sw.WriteLine("SYNC");
                                pipeServer.WaitForPipeDrain();
                                // Send the console input to the client process.
                                Logger.Default.WriteLog("[PIPE] Sending source- and destPath...");
                                sw.WriteLine(string.Format("-sourcePath={0} -destPath={1}{2}", sourcePath, destPath, App.Settings.RunCommandPromptHidden ? " -silent" : ""));
                            }
                        }
                        // Catch the IOException that is raised if the pipe is broken
                        // or disconnected.
                        catch (IOException ex)
                        {
                            Logger.Default.WriteLog("[PIPE] Error sending message to pipe client", ex);
                        }
                    }

                    pipeClient.WaitForExit();
                    pipeClient.Close();
                    Console.WriteLine("[PIPE] Client quit, session terminated.");
                }
            }).Start();
        }

        public static bool CompareLayoutHashes(string originalFilePath, string current)
        {
            if (!File.Exists(originalFilePath))
            {
                return false;
            }
            return !GetSHA256Hash(File.ReadAllText(originalFilePath)).Equals(GetSHA256Hash(current));
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

        private static string GetSHA256Hash(string value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return string.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }

        public static void KillLayoutTransferScript()
        {
            foreach (Process process in Process.GetProcessesByName("LayoutTransfer"))
            {
                process.Kill();
            }
        }

        private static bool IsLayoutTransferScriptRunning()
        {
            return Process.GetProcessesByName("LayoutTransfer").Any();
        }

    }
}
