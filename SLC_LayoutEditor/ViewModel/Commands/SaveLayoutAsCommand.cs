using SLC_LayoutEditor.ViewModel.Communication;
using System.Linq;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class SaveLayoutAsCommand : LayoutBaseCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is LayoutEditorViewModel vm)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.Keybind_SaveLayoutAs, 
                    vm.SelectedLayoutSet.CabinLayouts.Select(x => x.LayoutName));
            }
        }
    }
}
