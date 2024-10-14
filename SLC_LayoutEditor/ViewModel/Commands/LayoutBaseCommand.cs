using Tasty.ViewModel.Commands;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class LayoutBaseCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is LayoutEditorViewModel vm &&
                vm.ActiveLayout != null;
        }
    }
}
