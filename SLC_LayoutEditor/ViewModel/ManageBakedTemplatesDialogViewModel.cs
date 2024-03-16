using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Tasty.Logging;

namespace SLC_LayoutEditor.ViewModel
{
    internal class ManageBakedTemplatesDialogViewModel : BaseDialogViewModel
    {
        private List<BakedTemplateData> bakedTemplates = new List<BakedTemplateData>();
        private BakedTemplateData mSelectedBakedTemplate;
        private int mSelectedDeckThumbnailIndex;

        public List<BakedTemplateData> BakedTemplates => bakedTemplates;

        public BakedTemplateData SelectedBakedTemplate
        {
            get => mSelectedBakedTemplate;
            set
            {
                mSelectedBakedTemplate = value;
                /*if (!mSelectedBakedTemplate?.IsPreviewLoaded ?? true)
                {
                    mSelectedBakedTemplate.LoadPreview();
                }*/

                InvokePropertyChanged();
                /*SelectedDeckThumbnailIndex = 0;
                InvokePropertyChanged(nameof(HasMultipleDecks));
                InvokePropertyChanged(nameof(HasSelectedTemplateThumbnails));
                InvokePropertyChanged(nameof(IsTemplateSelected));*/
            }
        }

        /*public bool HasMultipleDecks => mSelectedBakedTemplate?.Preview.Thumbnails.Count > 1;

        public int SelectedDeckThumbnailIndex
        {
            get => mSelectedDeckThumbnailIndex;
            set
            {
                mSelectedDeckThumbnailIndex = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SelectedDeckThumbnail));
            }
        }

        public ImageSource SelectedDeckThumbnail => mSelectedBakedTemplate?.Preview.GetThumbnailForDeck(mSelectedDeckThumbnailIndex);

        public bool HasSelectedTemplateThumbnails => mSelectedBakedTemplate?.Preview.HasThumbnails ?? false;

        public bool IsTemplateSelected => mSelectedBakedTemplate != null;*/

        public ManageBakedTemplatesDialogViewModel()
        {
            foreach (string bakedTemplatePath in Util.GetBakedTemplates())
            {
                bakedTemplates.Add(new BakedTemplateData(bakedTemplatePath));
            }

            bakedTemplates = bakedTemplates.OrderBy(x => x.Exists).ToList();
        }
    }
}
