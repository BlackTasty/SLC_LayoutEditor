using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class RenameLayoutCommand : LayoutBaseCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is LayoutEditorViewModel vm &&
                vm.ActiveLayout is CabinLayout target)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.EditLayoutNameRequested, target);
            }
        }
    }
}
