using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    public class AddEditDialogViewModel : BaseDialogViewModel
    {
        private string nameExistsErrorMessage;

        private string mName;
        private List<string> mExistingNames = new List<string>();

        public string NameExistsErrorMessage
        {
            get => nameExistsErrorMessage;
            set
            {
                nameExistsErrorMessage = value;
                InvokePropertyChanged();
            }
        }

        public string Name
        {
            get => mName;
            set
            {
                mName = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsValid));
                InvokePropertyChanged(nameof(ErrorMessage));
            }
        }

        public override bool IsValid => !string.IsNullOrWhiteSpace(mName) && !mExistingNames.Contains(mName);

        public string ErrorMessage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mName))
                {
                    return "Name cannot be empty!";
                }
                else if (mExistingNames.Contains(mName))
                {
                    return nameExistsErrorMessage;
                }

                return "";
            }
        }

        public List<string> ExistingNames
        {
            get => mExistingNames;
            set
            {
                mExistingNames = value;
                InvokePropertyChanged();
            }
        }

        public AddEditDialogViewModel(string nameExistsErrorMessage, string defaultName = null)
        {
            mName = defaultName;
            this.nameExistsErrorMessage = nameExistsErrorMessage;
        }

        public void RefreshIsValidFlag()
        {
            InvokePropertyChanged(nameof(IsValid));
        }
    }
}
