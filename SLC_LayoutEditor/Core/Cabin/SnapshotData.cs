using SLC_LayoutEditor.Core.Cabin.Renderer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace SLC_LayoutEditor.Core.Cabin
{
    internal class SnapshotData
    {
        private FileInfo snapshotFile;

        private string fileContent;
        private DateTime creationDateTime;
        private List<ImageSource> thumbnails = new List<ImageSource>();
        private List<string> deckNames = new List<string>();
        private CabinLayout snapshot;
        private bool thumbnailsGenerated;

        private bool isRemoved;

        public string FileContent => fileContent;

        public DateTime CreationDateTime => creationDateTime;

        public List<ImageSource> Thumbnails => thumbnails;

        public List<string> DeckNames => deckNames;

        public bool HasThumbnails => thumbnails.Count > 0;

        public bool IsRemoved => isRemoved;

        public SnapshotData(string snapshotPath)
        {
            snapshotFile = new FileInfo(snapshotPath);
            string[] fileNameData = snapshotFile.Name.Split('.');

            if (fileNameData.Length > 2 && 
                DateTime.TryParseExact(fileNameData[1], "yyyy-MM-dd_HH-mm-ss",
                    CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime creationDateTime))
            {
                this.creationDateTime = creationDateTime;
            }

            snapshot = new CabinLayout(snapshotFile);
            fileContent = File.ReadAllText(snapshotPath);
        }

        public ImageSource GetThumbnailForDeck(int index)
        {
            if (!thumbnailsGenerated)
            {
                snapshot.LoadLayoutData(true);
                foreach (CabinDeck cabinDeck in snapshot.CabinDecks)
                {
                    CabinDeckRenderer renderer = new CabinDeckRenderer(cabinDeck);
                    thumbnails.Add(renderer.GenerateThumbnail());
                    deckNames.Add(Util.GetFloorName(thumbnails.Count));
                }
                thumbnailsGenerated = true;
            }
            return index >= 0 && HasThumbnails && thumbnails.Count > index ? thumbnails[index] : null;
        }

        public void Delete()
        {
            if (isRemoved)
            {
                return;
            }

            snapshotFile.Delete();
            isRemoved = true;
        }
    }
}
