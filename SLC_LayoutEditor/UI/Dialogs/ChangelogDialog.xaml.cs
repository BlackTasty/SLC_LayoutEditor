using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using System.Windows;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangelogDialog.xaml
    /// </summary>
    public partial class ChangelogDialog : DialogBase
    {
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
            CancelDialog();
        }

        protected override void OnDialogClosing(DialogClosingEventArgs e)
        {
            if (App.Settings.ShowChangesAfterUpdate != oldShowState)
            {
                App.SaveAppSettings();
            }

            base.OnDialogClosing(e);
        }

        public override void CancelDialog()
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK));
        }
    }
}
