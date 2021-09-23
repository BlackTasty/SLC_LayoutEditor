using SLC_LayoutEditor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private FrameworkElement mContent;

        public FrameworkElement Content
        {
            get => mContent;
            set
            {
                mContent = value;
                InvokePropertyChanged();
            }
        }

        public MainViewModel()
        {
            if (!App.Settings.WelcomeScreenShown)
            {
                WelcomeScreen welcome = new WelcomeScreen();
                welcome.WelcomeConfirmed += Welcome_WelcomeConfirmed;
                mContent = welcome;
            }
            else
            {
                mContent = new CabinConfig();
            }
        }

        private void Welcome_WelcomeConfirmed(object sender, EventArgs e)
        {
            if (mContent is WelcomeScreen welcome)
            {
                welcome.WelcomeConfirmed -= Welcome_WelcomeConfirmed;
            }

            Content = new CabinConfig();
        }
    }
}
