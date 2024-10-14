using SLC_LayoutEditor.ViewModel.Communication;
using Tasty.ViewModel.Commands;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal class ShowAboutDialogCommand : CommandBase
    {
        public override void Execute(object parameter)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.About_Show);
        }
    }
}
