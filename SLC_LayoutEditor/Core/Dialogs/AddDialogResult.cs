namespace SLC_LayoutEditor.Core.Dialogs
{
    public class AddDialogResult
    {
        private readonly string name;
        private readonly string selectedTemplatePath;
        private readonly bool isCreate;

        public string Name => name;

        public string SelectedTemplatePath => selectedTemplatePath;

        public bool IsCreate => isCreate;

        public AddDialogResult(string name, string selectedTemplatePath) : this(name)
        {
            this.selectedTemplatePath = selectedTemplatePath;
        }

        public AddDialogResult(string name) : this(true)
        {
            this.name = name;
        }

        public AddDialogResult(bool isCreate)
        {
            this.isCreate = isCreate;
        }
    }
}
