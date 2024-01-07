using SLC_LayoutEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    internal class TodoListDesignViewModel : TodoListViewModel
    {
        public TodoListDesignViewModel()
        {
            TodoEntries.Add(new TodoEntry("First entry", 5));
            TodoEntries.Add(new TodoEntry("Second entry", 2)
            {
                Current = 2
            });
            TodoEntries.Add(new TodoEntry("Third entry", 3)
            {
                Current = 2
            });
            TodoEntries.Add(new TodoEntry("Fourth entry"));
            TodoEntries.Add(new TodoEntry("Fifth entry"));
            TodoEntries.Last().ForceComplete(true);
            TodoEntries.Add(new TodoEntry("Optional sixth entry", true));
        }
    }
}
