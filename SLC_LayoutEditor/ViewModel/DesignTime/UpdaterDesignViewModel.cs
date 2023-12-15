using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    internal class UpdaterDesignViewModel : UpdaterViewModel
    {
        public UpdaterDesignViewModel()
        {
            App.InitPatcher();
            IsSearching = true;
            ShowButton = true;
            UpdateStatus = Core.Patcher.UpdateStatus.READY;
        }
    }
}
