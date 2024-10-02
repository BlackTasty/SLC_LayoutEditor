using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Commands;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class ToggleLogOutputCommand : CommandBase
    {
        public override bool CanExecute(object parameter)
        {
            return App.IsDebugMode && parameter is MainViewModel;
        }

        public override void Execute(object parameter)
        {
            if (parameter is MainViewModel vm)
            {
                vm.ShowLogOutput = !vm.ShowLogOutput;
            }
        }
    }
}
