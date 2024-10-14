using SLC_LayoutEditor.Core;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class SettingsViewModel : ViewModelBase
    {
        public AppSettings Settings => App.Settings;

        public void Refresh()
        {
            InvokePropertyChanged(nameof(Settings));
        }
    }
}
