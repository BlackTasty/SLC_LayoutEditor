namespace SLC_LayoutEditor.Core.Dialogs
{
    public class AddDialogResult
    {
        private readonly string name;
        private readonly bool isCreate;

        public string Name => name;

        public bool IsCreate => isCreate;

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
