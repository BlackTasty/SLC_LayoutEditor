using Tasty.ViewModel.Commands;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class HistoryCommandBase : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is MainViewModel vm && vm.AllowHistoryCommands;
        }
    }
}
