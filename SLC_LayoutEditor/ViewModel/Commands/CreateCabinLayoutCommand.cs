using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Commands;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class CreateCabinLayoutCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is LayoutEditorViewModel vm &&
                vm.SelectedLayoutSet != null;
        }

        public override void Execute(object parameter)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.Keybind_Begin_CreateLayoutOrTemplate, false);
        }
    }
}
