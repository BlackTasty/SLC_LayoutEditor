using SLC_LayoutEditor.ViewModel.Communication;
using System.Diagnostics;
using System.Windows;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : DialogBase
    {
        public AboutDialog()
        {
            InitializeComponent();
            title.Text += string.Format(" ({0})", App.GetVersionText());
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CancelDialog();
        }

        private void SLC_1_6_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.selfloadingcargo.com/news/17/self-loading-cargo-v16--is-released/");
        }

        private void SLC_Website_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.selfloadingcargo.com/");
        }

        private void Roadmap_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://trello.com/b/vJMbqwXb/slc-layout-editor-roadmap");
        }

        private void Changelog_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Mediator.Instance.NotifyColleagues(ViewModelMessage.Changelog_Show);
            CancelDialog();
        }
    }
}
