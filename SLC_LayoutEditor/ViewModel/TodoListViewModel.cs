using SLC_LayoutEditor.Core;
using System.Linq;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    internal class TodoListViewModel : ViewModelBase
    {
        private VeryObservableCollection<TodoEntry> mTodoEntries = new VeryObservableCollection<TodoEntry>(nameof(TodoEntries));

        public VeryObservableCollection<TodoEntry> TodoEntries
        {
            get { return mTodoEntries; }
            set
            {
                mTodoEntries = value;
                InvokePropertyChanged();
            }
        }

        public bool HasEntries => mTodoEntries.Count > 0;

        public bool AllEntriesComplete => TodoEntries.Where(x => !x.IsOptional).All(x => x.IsComplete);

        internal void SetTodoList(params TodoEntry[] entries)
        {
            if (entries != null)
            {
                ClearTodoList(false);

                AddTodoListEntries(entries);
            }
        }

        internal void AddTodoListEntries(params TodoEntry[] entries)
        {
            TodoEntries.AddRange(entries);
            InvokePropertyChanged(nameof(HasEntries));
        }

        internal void ClearTodoList()
        {
            ClearTodoList(true);
        }

        private void ClearTodoList(bool informUI)
        {
            if (TodoEntries.Any())
            {
                TodoEntries.Clear();

                if (informUI)
                {
                    InvokePropertyChanged(nameof(HasEntries));
                }
            }
        }
    }
}
