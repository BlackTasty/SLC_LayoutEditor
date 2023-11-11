using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class AddCabinLayoutDialogViewModel : ViewModelBase
    {
        private string mName = "Default";
        private List<string> mExistingLayoutNames = new List<string>();

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

        public bool IsValid => !string.IsNullOrWhiteSpace(mName) && !mExistingLayoutNames.Contains(mName);

        public string ErrorMessage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mName))
                {
                    return "Name cannot be empty!";
                }
                else if (mExistingLayoutNames.Contains(mName))
                {
                    return "A cabin layout with this name exists already!";
                }

                return "";
            }
        }

        public List<string> ExistingLayoutNames
        {
            get => mExistingLayoutNames;
            set
            {
                mExistingLayoutNames = value;
                InvokePropertyChanged();
            }
        }
    }
}
