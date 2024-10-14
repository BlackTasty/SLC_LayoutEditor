using System.IO;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    internal class BakedTemplateData : ViewModelBase
    {
        private bool exists;
        private readonly string aircraftName;
        private readonly string layoutCode;
        private readonly string targetDirectoryPath;
        private readonly string targetFilePath;
        private TemplatePreview preview;
        private bool isPreviewLoaded;

        public bool Exists => exists;

        public string AircraftName => aircraftName;

        public string LayoutCode => layoutCode;

        public string TargetDirectoryPath => targetDirectoryPath;

        public string TargetFilePath => targetFilePath;

        public TemplatePreview Preview => preview;
        
        public bool IsPreviewLoaded => isPreviewLoaded;

        public string FileName => "Default.txt";

        public BakedTemplateData(string bakedTemplatePath)
        {
            string template = Util.ReadTextResource(bakedTemplatePath);
            int newLineIndex = template.IndexOf("\r\n");

            aircraftName = template.Substring(0, newLineIndex);
            targetDirectoryPath = App.GetTemplatePath(aircraftName);
            targetFilePath = Path.Combine(targetDirectoryPath, FileName);
            CheckIfExists();

            layoutCode = exists ? File.ReadAllText(targetFilePath) :
                template.Substring(newLineIndex + 2);
        }

        public void CheckIfExists()
        {
            exists = File.Exists(targetFilePath);
            InvokePropertyChanged(nameof(Exists));
        }

        public void LoadPreview()
        {
            if (!isPreviewLoaded)
            {
                preview = new TemplatePreview(new CabinLayout(layoutCode, targetFilePath, FileName));

                if (preview != null)
                {
                    preview.GenerateThumbnails();
                }
                InvokePropertyChanged(nameof(Preview));
                isPreviewLoaded = true;
            }
        }

        public override string ToString()
        {
            return aircraftName;
        }
    }
}
