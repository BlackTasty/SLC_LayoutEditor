using System.Collections.Generic;

namespace SLC_LayoutEditor.ViewModel
{
    class CreateCabinLayoutDialogViewModel : AddDialogViewModel
    {
        private List<string> mTemplates = new List<string>();

        public List<string> Templates
        {
            get => mTemplates;
            set
            {
                mTemplates = value;
                InvokePropertyChanged();
            }
        }

        public CreateCabinLayoutDialogViewModel() : 
            base("A cabin layout with this name exists already!", "Default")
        {

        }
    }
}
