using SLC_LayoutEditor.Core.Patcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : DialogBase
    {
        public About()
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
    }
}
