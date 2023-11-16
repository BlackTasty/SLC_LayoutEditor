using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class AddDialogViewModel : ViewModelBase
    {
        private readonly string nameExistsErrorMessage;

        private string mName;
        private List<string> mExistingNames = new List<string>();

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

        public bool IsValid => !string.IsNullOrWhiteSpace(mName) && !mExistingNames.Contains(mName);

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

        public AddDialogViewModel(string nameExistsErrorMessage, string defaultName = null)
        {
            mName = defaultName;
            this.nameExistsErrorMessage = nameExistsErrorMessage;
        }
    }
}
