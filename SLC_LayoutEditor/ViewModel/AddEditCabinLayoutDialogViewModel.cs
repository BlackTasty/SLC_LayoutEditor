using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel
{
    public class AddEditCabinLayoutDialogViewModel : AddEditDialogViewModel
    {
        private bool mIsTemplate;

        public bool IsTemplate
        {
            get => mIsTemplate;
            set
            {
                mIsTemplate = value;
                InvokePropertyChanged();

                if (value)
                {
                    NameExistsErrorMessage = "A template with this name exists already!";
                }
            }
        }

        public AddEditCabinLayoutDialogViewModel() : this("A cabin layout with this name exists already!") { }

        public AddEditCabinLayoutDialogViewModel(string nameExistsErrorMessage, string defaultName = null) : base(nameExistsErrorMessage, defaultName) { }
    }
}
