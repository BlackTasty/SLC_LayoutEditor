using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class CreateTemplateCommand : CreateCabinLayoutCommand
    {
        public override void Execute(object parameter)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.Keybind_Begin_CreateLayoutOrTemplate, true);
        }
    }
}
