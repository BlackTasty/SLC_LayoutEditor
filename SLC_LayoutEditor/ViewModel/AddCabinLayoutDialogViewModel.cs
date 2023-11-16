using System.Collections.Generic;

namespace SLC_LayoutEditor.ViewModel
{
    class AddCabinLayoutDialogViewModel : AddDialogViewModel
    {
        private List<string> mTemplates = new List<string>();

        public AddCabinLayoutDialogViewModel() : 
            base("A cabin layout with this name exists already!", "Default")
        {

        }
    }
}
