﻿using SLC_LayoutEditor.Core.Cabin.Renderer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Tasty.Logging;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class TemplatePreview : ViewModelBase
    {
        private readonly CabinLayout template;

        private readonly string templateName;
        private readonly string templatePath;
        private readonly List<string> thumbnailPaths = new List<string>();
        private readonly List<ImageSource> thumbnails = new List<ImageSource>();
        private readonly List<string> deckNames = new List<string>();

        private readonly bool isDefault;

        public string TemplateName => templateName;

        public string TemplatePath => templatePath;

        //public string ThumbnailPath => thumbnailPath;

        public List<ImageSource> Thumbnails => thumbnails;

        public List<string> DeckNames => deckNames;

        public bool HasThumbnails => thumbnails.Count > 0 &&
            thumbnailPaths.All(x => File.Exists(x));

        public bool IsDefault => isDefault;

        public TemplatePreview(CabinLayout template)
        {
            this.template = template;

            templateName = template.LayoutName;
            templatePath = template.FilePath;
            //thumbnailPath = template.ThumbnailDirectory;
            LoadThumbnails();
        }

        public TemplatePreview()
        {
            isDefault = true;
            templateName = "Start from scratch";
        }

        public void GenerateThumbnails(bool overwrite = false)
        {
            if (isDefault)
            {
                return;
            }

            Logger.Default.WriteLog("Generating thumbnails...");
            if (!template.IsLoaded)
            {
                Logger.Default.WriteLog("Loading {0} code...", template.IsTemplate ? "template" : "layout");
                template.LoadLayoutData();
            }

            foreach (CabinDeck cabinDeck in template.CabinDecks)
            {
                CabinDeckRenderer renderer = new CabinDeckRenderer(cabinDeck);
                renderer.GenerateThumbnail(overwrite);
            }

            Logger.Default.WriteLog("Thumbnails generated");
            LoadThumbnails();
            InvokePropertyChanged(nameof(HasThumbnails));
            InvokePropertyChanged(nameof(Thumbnails));
            InvokePropertyChanged(nameof(DeckNames));
        }

        public ImageSource GetThumbnailForDeck(int index)
        {
            return HasThumbnails && thumbnails.Count > index ? thumbnails[index] : null;
        }

        private void LoadThumbnails()
        {
            Logger.Default.WriteLog("Loading thumbnails from disk...");
            thumbnailPaths.Clear();
            deckNames.Clear();
            thumbnails.Clear();
            if (Directory.Exists(template.ThumbnailDirectory))
            {
                foreach (FileInfo thumbnailFile in new DirectoryInfo(template.ThumbnailDirectory).EnumerateFiles("*.png"))
                {
                    if (int.TryParse(thumbnailFile.Name.Replace(".png", ""), out int floor))
                    {
                        thumbnailPaths.Add(thumbnailFile.FullName);
                        deckNames.Add(Util.GetFloorName(floor));
                        thumbnails.Add(Util.LoadImage(thumbnailFile.FullName));
                    }
                }
            }
            Logger.Default.WriteLog("{0} thumbnails have been loaded for {1} decks", thumbnails.Count, template.CabinDecks.Count);
        }

        public override string ToString()
        {
            return templateName;
        }
    }
}
