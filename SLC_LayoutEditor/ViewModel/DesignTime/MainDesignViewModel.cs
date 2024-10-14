using SLC_LayoutEditor.UI;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    class MainDesignViewModel : MainViewModel
    {
        public MainDesignViewModel() : base()
        {
            //Message = new WelcomeScreen();
            Content = new LayoutEditor();
            //Dialog = new ChangelogDialog();
        }
    }
}
