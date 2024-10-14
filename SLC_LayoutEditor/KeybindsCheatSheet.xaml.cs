using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Windows;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for KeybindsCheatSheet.xaml
    /// </summary>
    public partial class KeybindsCheatSheet : Window
    {
        public KeybindsCheatSheet()
        {
            InitializeComponent();
            App.IsKeybindSheetOpen = true;

            Mediator.Instance.Register(o =>
            {
                Focus();
            }, ViewModelMessage.RefocusKeybindSheet);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.IsKeybindSheetOpen = false;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void header_Loaded(object sender, RoutedEventArgs e)
        {
            InitHeader(sender as UIElement);
        }

        private void InitHeader(UIElement header)
        {
            var restoreIfMove = false;

            header.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount != 2)
                {
                    DragMove();
                }
            };
            header.MouseLeftButtonUp += (s, e) =>
            {
                restoreIfMove = false;
            };
            header.MouseMove += (s, e) =>
            {
                if (restoreIfMove)
                {
                    restoreIfMove = false;
                    var mouseX = e.GetPosition(this).X;
                    var width = RestoreBounds.Width;
                    var x = mouseX - width / 2;

                    if (x < 0)
                    {
                        x = 0;
                    }
                    else
                    if (x + width > SystemParameters.PrimaryScreenWidth)
                    {
                        x = SystemParameters.PrimaryScreenWidth - width;
                    }

                    WindowState = WindowState.Normal;
                    Left = x;
                    Top = 0;
                    DragMove();
                }
            };
        }
    }
}
