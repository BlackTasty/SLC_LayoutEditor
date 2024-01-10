using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    class MainDesignViewModel : MainViewModel
    {
        public MainDesignViewModel() : base()
        {
            //Content = new WelcomeScreen();
            Content = new LayoutEditor();
            //Dialog = new ChangelogDialog();
        }
    }
}
