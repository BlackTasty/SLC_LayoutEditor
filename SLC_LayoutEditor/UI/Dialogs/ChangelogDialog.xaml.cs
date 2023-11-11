using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangelogDialog.xaml
    /// </summary>
    public partial class ChangelogDialog : DockPanel
    {
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        private bool oldShowState;

        public ChangelogDialog()
        {
            InitializeComponent();
            oldShowState = App.Settings.ShowChangesAfterUpdate;
        }

        private void Container_Loaded(object sender, RoutedEventArgs e)
        {
            changelog.Initialize(new Core.Patcher.ChangelogEntry(Util.ReadTextResource("patchnotes.txt")));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK));
        }

        protected virtual void OnDialogClosing(DialogClosingEventArgs e)
        {
            if (App.Settings.ShowChangesAfterUpdate != oldShowState)
            {
                App.SaveAppSettings();
            }
            DialogClosing?.Invoke(this, e);
        }
    }
}
