namespace SLC_LayoutEditor.ViewModel
{
    public class AddEditCabinLayoutDialogViewModel : AddEditDialogViewModel
    {
        private bool mIsTemplate;
        private string mTitle = "Edit cabin layout name";
        private string mInputTitle = "Layout name";

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

        public string Title
        {
            get => mTitle;
            set
            {
                mTitle = value;
                InvokePropertyChanged();
            }
        }

        public string InputTitle
        {
            get => mInputTitle;
            set
            {
                mInputTitle = value;
                InvokePropertyChanged();
            }
        }

        public AddEditCabinLayoutDialogViewModel() : this("An entry with this name exists already!") { }

        public AddEditCabinLayoutDialogViewModel(string nameExistsErrorMessage, string defaultName = null) : base(nameExistsErrorMessage, defaultName) { }
    }
}
