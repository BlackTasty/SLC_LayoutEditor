﻿using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SLC_LayoutEditor.UI
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : DockPanel
    {
        public event EventHandler<EventArgs> WelcomeConfirmed;

        public WelcomeScreen()
        {
            InitializeComponent();
        }

        protected virtual void OnWelcomeConfirmed(EventArgs e)
        {
            App.Settings.WelcomeScreenShown = true;
            App.SaveAppSettings();
            WelcomeConfirmed?.Invoke(this, e);
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            OnWelcomeConfirmed(EventArgs.Empty);
        }
    }
}
