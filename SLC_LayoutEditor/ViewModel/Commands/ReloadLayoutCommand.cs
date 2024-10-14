using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class ReloadLayoutCommand : LayoutBaseCommand
    {
        public override void Execute(object parameter)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.Keybind_ReloadLayout);
            /*if (parameter is LayoutEditorViewModel vm &&
                vm.ActiveLayout is CabinLayout target)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Keybind_ReloadLayout, target);
            }*/
        }
    }
}
