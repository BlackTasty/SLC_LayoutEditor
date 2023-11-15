﻿using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Dialogs;
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
    /// Interaction logic for AddAirplaneDialog.xaml
    /// </summary>
    public partial class AddAirplaneDialog : DockPanel, IDialog
    {
        public event EventHandler<DialogClosingEventArgs> DialogClosing;

        public AddAirplaneDialog(IEnumerable<string> airplaneNames)
        {
            InitializeComponent();
            (DataContext as AddAirplaneDialogViewModel).ExistingNames.AddRange(airplaneNames);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.OK, new AddDialogResult((DataContext as AddAirplaneDialogViewModel).Name)));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OnDialogClosing(new DialogClosingEventArgs(DialogResultType.Cancel, new AddDialogResult(false)));
        }

        protected virtual void OnDialogClosing(DialogClosingEventArgs e)
        {
            DialogClosing?.Invoke(this, e);
        }
    }
}
