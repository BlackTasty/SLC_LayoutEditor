using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class OpenLayoutInTextEditor : LayoutBaseCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is LayoutEditorViewModel vm &&
                vm.ActiveLayout is CabinLayout target)
            {
                Util.OpenFile(target.FilePath);
            }
        }
    }
}
