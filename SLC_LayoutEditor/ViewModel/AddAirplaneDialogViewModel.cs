using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class AddAirplaneDialogViewModel : ViewModelBase
    {
        private string mName;
        private List<string> mExistingNames = new List<string>();

        public string Name
        {
            get => mName;
            set
            {
                mName = value;
                InvokePropertyChanged();
                InvokePropertyChanged("IsValid");
                InvokePropertyChanged("ErrorMessage");
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
                    return "An airplane with this name exists already!";
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
    }
}
