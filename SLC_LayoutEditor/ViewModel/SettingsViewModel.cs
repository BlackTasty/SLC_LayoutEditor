using SLC_LayoutEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class SettingsViewModel : ViewModelBase
    {
        public AppSettings Settings => App.Settings;
    }
}
