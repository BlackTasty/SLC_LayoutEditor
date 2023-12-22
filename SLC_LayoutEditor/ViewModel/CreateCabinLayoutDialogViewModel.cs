using SLC_LayoutEditor.Core.Cabin;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace SLC_LayoutEditor.ViewModel
{
    class CreateCabinLayoutDialogViewModel : AddEditCabinLayoutDialogViewModel
    {
        private List<TemplatePreview> mTemplates = new List<TemplatePreview>();
        private TemplatePreview mSelectedTemplate;
        private int mSelectedDeckThumbnailIndex = -1;

        private bool mIsSaveAs;

        public List<TemplatePreview> Templates
        {
            get => mTemplates;
            set
            {
                value.Insert(0, new TemplatePreview());
                SelectedTemplate = value.First();
                mTemplates = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(HasTemplates));
            }
        }

        public TemplatePreview SelectedTemplate
        {
            get => mSelectedTemplate;
            set
            {
                mSelectedTemplate = value;
                SelectedDeckThumbnailIndex = value.HasThumbnails ? 0 : -1;

                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SelectedTemplateText));
                InvokePropertyChanged(nameof(IsTemplateSelected));
                InvokePropertyChanged(nameof(HasTemplateThumbnails));
                InvokePropertyChanged(nameof(SelectedDeckThumbnail));
                InvokePropertyChanged(nameof(HasTemplateMultipleDecks));
            }
        }

        public bool HasTemplateMultipleDecks => mSelectedTemplate?.Thumbnails.Count > 1;

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

        public bool IsSaveAs
        {
            get => mIsSaveAs;
            set
            {
                mIsSaveAs = value;
                InvokePropertyChanged();
            }
        }

        public ImageSource SelectedDeckThumbnail => SelectedTemplate?.GetThumbnailForDeck(mSelectedDeckThumbnailIndex);

        public bool IsTemplateSelected => SelectedTemplate != null;

        public string SelectedTemplateText => mSelectedTemplate != null ?
            mSelectedTemplate.TemplateName : "Start from scratch";

        public bool HasTemplates => mTemplates.Count > 0;

        public bool HasTemplateThumbnails => mSelectedTemplate?.HasThumbnails ?? false;

        public CreateCabinLayoutDialogViewModel() : 
            base("A cabin layout with this name exists already!", "Default")
        {

        }
    }
}
