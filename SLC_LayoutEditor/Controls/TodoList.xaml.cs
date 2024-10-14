using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.ViewModel;
using System.Windows.Controls;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for TodoList.xaml
    /// </summary>
    public partial class TodoList : UserControl
    {
        private TodoListViewModel vm;

        public bool AllEntriesComplete => vm?.AllEntriesComplete ?? true;

        public TodoList()
        {
            InitializeComponent();
            vm = DataContext as TodoListViewModel;
        }

        internal void AddTodoListEntries(params TodoEntry[] entries)
        {
            vm.AddTodoListEntries(entries);
        }

        internal void SetTodoListEntries(params TodoEntry[] entries)
        {
            vm.SetTodoList(entries);
        }

        internal bool UpdateEntry(int entryIndex, int currentAmount)
        {
            vm.TodoEntries[entryIndex].Current = currentAmount;

            return vm.TodoEntries[entryIndex].IsComplete;
        }

        internal void ForceCompleteEntry(int entryIndex, bool isComplete)
        {
            vm.TodoEntries[entryIndex].ForceComplete(isComplete);
        }

        internal int GetCurrentAmountForEntry(int entryIndex)
        {
            return vm.TodoEntries[entryIndex].Current;
        }

        internal bool GetIsCompleteForEntry(int entryIndex)
        {
            return vm.TodoEntries[entryIndex].IsComplete;
        }

        internal void ClearTodoList()
        {
            vm.ClearTodoList();
        }
    }
}
