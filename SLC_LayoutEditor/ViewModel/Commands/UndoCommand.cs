namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class UndoCommand : HistoryCommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && parameter is MainViewModel vm && vm.History.CanUndo;
        }

        public override void Execute(object parameter)
        {
            if (parameter is MainViewModel vm && vm.History.CanUndo)
            {
                vm.History.Undo();
            }
        }
    }
}
