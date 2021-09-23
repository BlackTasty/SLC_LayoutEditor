using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class AddDialogClosingEventArgs : EventArgs
    {
        private string name;
        private bool isCreate;

        public string Name => name;

        public bool IsCreate => isCreate;

        public AddDialogClosingEventArgs(string name) : this(true)
        {
            this.name = name;
        }

        public AddDialogClosingEventArgs(bool isCreate)
        {
            this.isCreate = isCreate;
        }
    }
}
