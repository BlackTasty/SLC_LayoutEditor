using System;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    internal class SpecifyDeckSizeDialogViewModel : ViewModelBase
    {
        private int mRows = 1;
        private int mColumns = 1;

        public int Rows
        {
            get => mRows;
            set
            {
                mRows = Math.Max(1, value);
                InvokePropertyChanged();
            }
        }

        public int Columns
        {
            get => mColumns;
            set
            {
                mColumns = Math.Max(1, value);
                InvokePropertyChanged();
            }
        }
    }
}
