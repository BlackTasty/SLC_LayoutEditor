using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core
{
    internal class TodoEntry : ViewModelBase
    {
        private readonly string title;
        private readonly int amount;
        private int mCurrent;
        private readonly bool isOptional;

        public string Title => title;

        public int Amount => amount;

        public bool HasTargetAmount => amount > 0;

        public int Current
        {
            get { return mCurrent; }
            set
            {
                mCurrent = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(IsComplete));
            }
        }

        public bool IsComplete => mCurrent >= amount;

        public bool IsOptional => isOptional;

        public TodoEntry(string title, int amount, int current = 0, bool isOptional = false)
        {
            this.title = title;
            this.amount = amount;
            mCurrent = current;
            this.isOptional = isOptional;
        }

        public TodoEntry(string title, bool isOptional = false) : this(title, 0, -1, isOptional) { }

        public void ForceComplete(bool isComplete)
        {
            Current = isComplete ? 0 : -1;
            InvokePropertyChanged(nameof(IsComplete));
        }
    }
}
