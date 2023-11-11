﻿using SLC_LayoutEditor.Core;
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

namespace SLC_LayoutEditor.UI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : DockPanel
    {
        private SettingsViewModel vm;

        public Settings()
        {
            InitializeComponent();
            vm = DataContext as SettingsViewModel;
        }

        private void OpenReadoutFolder_Click(object sender, RoutedEventArgs e)
        {
            Util.OpenFolder(App.Settings.CabinLayoutsReadoutPath, false);
        }

        private void OpenEditFolder_Click(object sender, RoutedEventArgs e)
        {
            Util.OpenFolder(App.Settings.CabinLayoutsEditPath, false);
        }

        private void SelectSLCPath_Click(object sender, RoutedEventArgs e)
        {
            string path = Util.SelectFolder("Select SLC cabin layouts folder",
                                                        App.Settings.CabinLayoutsReadoutPath,
                                                        false);
            if (path != null)
            {
                App.Settings.CabinLayoutsReadoutPath = path;
            }
        }

        private void SelectCopyTargetPath_Click(object sender, RoutedEventArgs e)
        {
            string path = Util.SelectFolder("Select a folder to copy all layouts to",
                                                        App.Settings.CabinLayoutsEditPath,
                                                        true);
            if (path != null)
            {
                App.Settings.CabinLayoutsEditPath = path;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Save(App.Settings.FilePath);
        }

        private void Rollback_Click(object sender, RoutedEventArgs e)
        {
            App.Settings = AppSettings.Load(new System.IO.FileInfo(System.IO.Path.Combine(App.Settings.FilePath, App.Settings.FileName)));
        }
    }
}
