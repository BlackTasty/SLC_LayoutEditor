using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LayoutTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            SplitTempArgs(@"-sourcePath=C:\Users\rapha\Desktop\SLC Layout Editor\Tested\CmdrJk\A320_Swiss.txt -destPath=C:\Users\rapha\Desktop\SLC Layout Editor\In Testing\CmdrJk\A320_Swiss.txt");
#if DEBUG
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
#endif

            bool keepOpen = false;
            bool isSilent = false;

            for (int i = 0; i < args.Length; i++)
            {
                string[] argumentData = args[i].Split('=');

                switch (argumentData[0].ToLower())
                {
                    case "-silent":
                        isSilent = true;
                        break;
                    case "-keepopen":
                        keepOpen = true;
                        break;
                }
            }

            if (args.Length > 0)
            {
                bool shutdown = false;

                using (PipeStream pipeClient =
                    new AnonymousPipeClientStream(PipeDirection.In, args[0]))
                {
                    Console.WriteLine("[PIPE] Current TransmissionMode: {0}.",
                       pipeClient.TransmissionMode);

                    using (StreamReader sr = new StreamReader(pipeClient))
                    {
                        while (!shutdown)
                        {
                            // Display the read text to the console
                            string temp;

                            // Wait for 'sync message' from the server.
                            do
                            {
                                Console.WriteLine("[PIPE] Wait for sync...");
                                temp = sr.ReadLine();
                            }
                            while (temp == null || !temp.StartsWith("SYNC"));

                            // Read the server data and echo to the console.
                            while ((temp = sr.ReadLine()) != null)
                            {
                                if (temp == "-shutdown")
                                {
                                    shutdown = true;
                                    Console.WriteLine("[PIPE] Shutdown requested");
                                    break;
                                }

                                CopyLayouts(SplitTempArgs(temp));
                                Console.WriteLine("[PIPE] Echo: " + temp);
                            }
                        }
                    }
                }
            }

            if (!isSilent)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static string[] SplitTempArgs(string argsStr)
        {
            List<string> extractedArgs = new List<string>();

            string tempArgsStr = argsStr;
            while (tempArgsStr.Length > 0)
            {
                int extractionLength = tempArgsStr.IndexOf(" -");

                if (extractionLength > -1)
                {
                    StringBuilder argBuilder = new StringBuilder();

                    for (int i = 0; i < extractionLength; i++)
                    {
                        if (argBuilder.Length == 0 && tempArgsStr[i] == ' ')
                        {
                            continue;
                        }
                        argBuilder.Append(tempArgsStr[i]);
                    }

                    extractedArgs.Add(argBuilder.ToString());
                    tempArgsStr = tempArgsStr.Substring(extractionLength + 1);
                }
                else
                {
                    extractedArgs.Add(tempArgsStr.Trim());
                    break;
                }
            }

            return extractedArgs.ToArray();
        }

        private static void CopyLayouts(string[] args)
        {
            string sourcePath = null;
            string destPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                string[] argumentData = args[i].Split('=');

                switch (argumentData[0].ToLower())
                {
                    case "-sourcepath":
                        sourcePath = argumentData[1];
                        break;
                    case "-destpath":
                        destPath = argumentData[1];
                        break;
                }
            }

            if (sourcePath != null && destPath != null)
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
                else
                {
                    Console.WriteLine("[ERR] sourcePath does not point to an existing file!");
                }
            }
            else
            {
                Console.WriteLine("[ERR] sourcePath and destPath arguments must be supplied!");
            }
        }
    }
}
