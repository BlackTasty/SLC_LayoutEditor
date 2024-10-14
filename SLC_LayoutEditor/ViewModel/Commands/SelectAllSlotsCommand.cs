using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class SelectAllSlotsCommand : LayoutBaseCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter is LayoutEditorViewModel vm)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.SelectAll_Layout);
            }
        }
    }
}
