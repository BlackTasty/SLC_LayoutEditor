namespace SLC_LayoutEditor.Core.Events
{
    public class ChangedEventArgs
    {
        private readonly bool unsavedChanges;

        public bool UnsavedChanges => unsavedChanges;

        public ChangedEventArgs(bool unsavedChanges)
        {
            this.unsavedChanges = unsavedChanges;
        }
    }
}
