using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Controls.Notifications;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Guide;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.ViewModel;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string MAXIMIZE_ICON = (string)App.Current.FindResource("WindowMaximize");
        private static readonly string RESTORE_ICON = (string)App.Current.FindResource("WindowRestore");

        private UIElement root;
        private MainViewModel vm;
        private bool isClosing;
        private bool forceClose;

        private LiveGuideAdorner currentGuideAdorner;

        public MainWindow()
        {
            Util.RefreshTheme(Application.Current);

            InitializeComponent();
            vm = DataContext as MainViewModel;
            vm.StateToggleButtonContent = MAXIMIZE_ICON;

            CheckCleanupFile();
            vm.ShowChangelogIfUpdated();

            Mediator.Instance.Register(o =>
            {
                if (isClosing && (bool)o)
                {
                    forceClose = true;
                    Close();
                }
            }, ViewModelMessage.UnsavedChangesDialogClosed);

            Mediator.Instance.Register(o =>
            {
                if (o is LiveGuideData data)
                {
                    SetGuideAdorner(data);
                    vm.UpdateTourStepper();
                }
            }, ViewModelMessage.GuideAdornerShowing);

            Mediator.Instance.Register(o =>
            {
                SetGuideAdorner(null);
            }, ViewModelMessage.GuideAdornerClosed);
        }

        private void CheckCleanupFile()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\cleanup.txt"))
            {
                Logger.Default.WriteLog("Editor has been updated, cleaning up old files...");
                Process[] procList = Process.GetProcessesByName("slc_layouteditor");

                int cycleCount = 0;
                while (Process.GetProcessesByName("slc_layouteditor").Length > 1)
                {
                    if (cycleCount == 3)
                    {
                        if (MessageBox.Show("Your old SLC Layout Editor instance needs to be closed in order to cleanup after updating!\n" +
                            "Shall I close them for you?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            foreach (Process proc in procList)
                            {
                                if (Process.GetCurrentProcess().Id != proc.Id)
                                {
                                    proc.Kill();
                                    Logger.Default.WriteLog("Terminated old instance of editor.");
                                }
                            }
                        }
                        else
                        {
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    Thread.Sleep(1000);
                    cycleCount++;
                }

                string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\cleanup.txt");
                int deletedFiles = 0;
                foreach (string path in lines)
                {
                    if (!Util.SafeDeleteFile(path))
                    {
                        deletedFiles++;
                    }
                }

                Logger.Default.WriteLog("Successfully deleted {0}/{1} files!", deletedFiles, lines.Length);
                Util.SafeDeleteFile(AppDomain.CurrentDomain.BaseDirectory + "\\cleanup.txt");
            }
        }

        private void OpenWelcomeScreen_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowWelcomeScreen();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowSettings();
        }

        private void ReturnToEditor_Click(object sender, RoutedEventArgs e)
        {
            vm.ReturnToEditor();
        }

        private void SearchUpdates_Click(object sender, RoutedEventArgs e)
        {
            App.Patcher.CheckForUpdates();
        }

        private void DownloadAndInstallUpdate_Click(object sender, RoutedEventArgs e)
        {
            App.Patcher.DownloadUpdate();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose && vm.CheckUnsavedChanges(true))
            {
                Topmost = true;
                Topmost = false;
                isClosing = true;
                e.Cancel = true;
                return;
            }

            vm.RememberLayout();
        }

        private void Changelog_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowChangelog();
        }

        private void Roadmap_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://trello.com/b/vJMbqwXb/slc-layout-editor-roadmap");
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            RefreshGuideAdorner();
            vm.StateToggleButtonContent = WindowState == WindowState.Normal ? MAXIMIZE_ICON : RESTORE_ICON;
            vm.IsMaximized = WindowState == WindowState.Maximized;
        }

        private void SetGuideAdorner(LiveGuideData data)
        {
            if (data?.GuidedElement != null)
            {
                currentGuideAdorner = (LiveGuideAdorner)LiveGuideAdorner.AttachAdorner(root, data.GuidedElement);
                currentGuideAdorner.Closed += CurrentGuideAdorner_Closed;
            }
            vm.IsGuideOpen = data != null;
        }

        private void CurrentGuideAdorner_Closed(object sender, LiveGuideClosedEventArgs e)
        {
            currentGuideAdorner.Closed -= CurrentGuideAdorner_Closed;
            currentGuideAdorner = null;
            if (App.GuidedTour.IsTourRunning)
            {
                if (!e.IsTourStepOffsetSet)
                {
                    App.GuidedTour.ContinueTour();
                }
                else
                {
                    App.GuidedTour.ContinueTour(true);
                }
            }
        }

        private void ForceCloseGuideAdorner()
        {
            if (currentGuideAdorner != null)
            {
                currentGuideAdorner.Closed -= CurrentGuideAdorner_Closed;
                UIElement adornedElement = currentGuideAdorner.AdornedElement;
                adornedElement.RemoveAdorner(currentGuideAdorner);
                vm.IsGuideOpen = false;
            }
        }

        private void RefreshGuideAdorner()
        {
            if (currentGuideAdorner != null)
            {
                currentGuideAdorner.Closed -= CurrentGuideAdorner_Closed;
                UIElement adornedElement = currentGuideAdorner.AdornedElement;
                adornedElement.RemoveAdorner(currentGuideAdorner);
                currentGuideAdorner = (LiveGuideAdorner)LiveGuideAdorner.AttachAdorner(adornedElement, currentGuideAdorner.GuidedElement);
                currentGuideAdorner.Closed += CurrentGuideAdorner_Closed;
            }
        }

        private void Updater_InstallUpdateClicked(object sender, EventArgs e)
        {
        }

        private void ToggleGuide_Click(object sender, RoutedEventArgs e)
        {
            if (!App.GuidedTour.IsTourRunning)
            {
                App.GuidedTour.StartTour();
            }
            else
            {
                ForceCloseGuideAdorner();
                App.GuidedTour.StopTour();
            }
        }

        private void InitHeader(UIElement header)
        {
            var restoreIfMove = false;

            header.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    if ((ResizeMode == ResizeMode.CanResize) ||
                        (ResizeMode == ResizeMode.CanResizeWithGrip))
                    {
                        SwitchState();
                    }
                }
                else
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        restoreIfMove = true;
                    }

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
                    Point screenPosition = PointToScreen(Mouse.GetPosition(this));
                    Point relativePosition = e.GetPosition(this);
                    double widthOffset = ActualWidth - RestoreBounds.Width;
                    bool isLeftSide = (relativePosition.X - widthOffset * 2.5) <= 0;

                    WindowState = WindowState.Normal;
                    double x = screenPosition.X - relativePosition.X;
                    double y = screenPosition.Y - relativePosition.Y;

                    if (!isLeftSide)
                    {
                        x += widthOffset;
                    }

                    Left = x;
                    Top = y;

                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        DragMove();
                    }
                }
            };
        }

        private void SwitchState()
        {
            WindowState = !vm.IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }

        private void header_Loaded(object sender, RoutedEventArgs e)
        {
            InitHeader(sender as UIElement);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleState_Click(object sender, RoutedEventArgs e)
        {
            SwitchState();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            root = sender as UIElement;
        }

        private void StepBackInTour_Click(object sender, RoutedEventArgs e)
        {
            ForceCloseGuideAdorner();
            App.GuidedTour.StepBack();
        }

        private void StepForwardInTour_Click(object sender, RoutedEventArgs e)
        {
            ForceCloseGuideAdorner();
            App.GuidedTour.StepForward();
        }

        private void ShowCurrentStep_Click(object sender, RoutedEventArgs e)
        {
            App.GuidedTour.ShowCurrentStepAgain();
        }
    }
}
