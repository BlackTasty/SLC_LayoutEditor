using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
