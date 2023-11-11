using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Patcher
{
    public class ChangelogEntry : IComparable /*, ITimelineEntry*/
    {
        private DateTime patchDate = new DateTime(1989, 1, 1);
        private string patchNumber = "0.0.0.0";
        private string[] patchContent = new string[] { "EMPTY" };
        private bool isSketch, isHotfix;

        private double timelineIndex;

        public DateTime PatchDate { get { return patchDate; } }

        public string PatchNumber { get { return patchNumber; } }

        public string[] PatchContent { get { return patchContent; } }

        public bool IsSketch { get { return isSketch; } }

        public bool IsHotfix { get { return isHotfix; } }

        public DateTime GetTimelinePosition()
        {
            return patchDate;
        }

        public ChangelogEntry(string content)
        {
            Initialize(content.Replace("\r\n", "\n").Split('\n'));
            patchNumber = App.GetVersionText(true);
        }

        public ChangelogEntry(FileInfo fi)
        {
            string path = fi.FullName;
            Initialize(File.ReadAllLines(path));
            patchNumber = fi.Name.Replace(".txt", "");
        }

        /// <summary>
        /// Purely used to create a dummy object
        /// </summary>
        /// <param name="version">The version of this dummy object</param>
        /// <param name="dummyData">If set to true, this dummy object will contain test data</param>
        public ChangelogEntry(string version, bool dummyData)
        {
            patchNumber = version;
            if (dummyData)
            {
                patchContent = new string[]
                {
                    "Added: Dummy",
                    "Changed: Dummy dummy",
                    "Fixed: Dummy",
                    "Re-Enabled: No"
                };
                SetDate("17.02.1998");
            }
        }

        private void Initialize(string[] content)
        {
            string[] initialData = content[0].Split(' ');

            SetDate(initialData[0]);

            patchContent = new string[content.Length - 1];
            for (int i = 1; i < content.Length; i++)
                patchContent[i - 1] = content[i];

            if (initialData.Length > 1)
            {
                isSketch = initialData[1].Contains("s") || initialData[1].Contains("n");
                isHotfix = initialData[1].Contains("h");
            }
        }

        private void SetDate(string dateRaw)
        {
            string[] raw = dateRaw.Replace(":", "").Split('.');
            patchDate = new DateTime(TryParse(raw[2], 1989), TryParse(raw[1], 1), TryParse(raw[0], 1));
        }

        public int CompareTo(object obj)
        {
            ChangelogEntry pd = (ChangelogEntry)obj;
            return patchDate.CompareTo(pd.patchDate);
        }

        public override string ToString()
        {
            if (patchNumber != null)
            {
                if (patchDate != null) //1.0.0.0 [Sketch] [(Hotfix)] - 01.01.2018
                    return string.Format("{0} {1} {2} - {3}",
                        patchNumber, IsSketch ? "Sketch" : "", IsHotfix ? "(Hotfix)" : "", patchDate.ToShortDateString());
                else //1.0.0.0 [Sketch] [(Hotfix)]
                    return string.Format("{0} {1} {2}",
                        patchNumber, IsSketch ? "Sketch" : "", IsHotfix ? "(Hotfix)" : "");
            }
            else
                return "EMPTY";
        }

        public string GetTooltip()
        {
            return patchNumber;
        }

        public void SetTimelineIndex(double value)
        {
            timelineIndex = value;
        }

        public double GetTimelineIndex()
        {
            return timelineIndex;
        }

        private static int TryParse(string value, int defaultValue)
        {
            double temp = TryParse(value, (double)defaultValue);
            if (temp > int.MaxValue)
            {
                return int.MaxValue;
            }
            else if (temp < int.MinValue)
            {
                return int.MinValue;
            }
            else
            {
                if (int.TryParse(value, out int result))
                    return result;
                else
                    return defaultValue;
            }
        }

        private static double TryParse(string value, double defaultValue)
        {
            if (double.TryParse(value, out double result))
                return result;
            else
                return defaultValue;
        }
    }
}