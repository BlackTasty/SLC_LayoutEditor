﻿using SLC_LayoutEditor.Controls;
using SLC_LayoutEditor.Controls.Guide;
using SLC_LayoutEditor.Controls.Notifications;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Guide;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.UI;
using SLC_LayoutEditor.UI.Dialogs;
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
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private UIElement root;

        private Button undoButton;
        private ContextMenu undoHistoryMenu;
        private Button redoButton;
        private ContextMenu redoHistoryMenu;

        private MainViewModel vm;
        private bool isClosing;
        private bool forceClose;

        private LiveGuideAdorner currentGuideAdorner;
        private Notification supportNotification;

        public MainWindow()
        {
            Util.RefreshTheme(Application.Current);

            InitializeComponent();
            vm = DataContext as MainViewModel;

            CheckCleanupFile();
            vm.ShowChangelogIfUpdated();
            vm.HistoryChanged += HistoryChanged;

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

            Mediator.Instance.Register(o =>
            {
                if (o is bool isLoading)
                {
                    taskbarItemInfo.ProgressState = isLoading ? System.Windows.Shell.TaskbarItemProgressState.Indeterminate : System.Windows.Shell.TaskbarItemProgressState.None;
                }
            }, ViewModelMessage.Layout_Loading);

            Mediator.Instance.Register(o =>
            {
                if (o is bool isPaused)
                {
                    taskbarItemInfo.ProgressState = isPaused ? System.Windows.Shell.TaskbarItemProgressState.Paused : System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                }
            }, ViewModelMessage.Layout_LoadingPaused);
        }

        private void HistoryChanged(object sender, HistoryChangedEventArgs<CabinHistoryEntry> e)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                if (e.IsClear)
                {
                    redoHistoryMenu.Items.Clear();
                    undoHistoryMenu.Items.Clear();
                    return;
                }

                bool isUndo = !e.IsRecorded ? e.IsUndo : !e.IsUndo;
                MoveEntries(e.PoppedHistory, e.IsRecorded ? e.IsUndo : !e.IsUndo,
                    isUndo ? undoHistoryMenu : redoHistoryMenu,
                    isUndo ? redoHistoryMenu : undoHistoryMenu);
                (vm.Content as LayoutEditor)?.control_layout.RefreshState(false);
            });
        }

        private void MoveEntries(IEnumerable<CabinHistoryEntry> poppedHistory, bool isUndo, ContextMenu sourceHistory, ContextMenu targetHistory)
        {
            foreach (CabinHistoryEntry poppedEntry in poppedHistory)
            {
                if (GetMenuHistoryEntry(sourceHistory.Items, poppedEntry) is MenuItem targetItem)
                {
                    sourceHistory.Items.Remove(targetItem);
                }

                targetHistory.Items.Insert(0, GenerateHistoryItem(poppedEntry, isUndo));
            }
        }

        private MenuItem GetMenuHistoryEntry(ItemCollection items, CabinHistoryEntry entry)
        {
            return items.OfType<MenuItem>().FirstOrDefault(x => x.DataContext is CabinHistoryEntry checkedEntry &&
                checkedEntry.Guid == entry.Guid);
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
            vm.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            vm.Redo();
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
            vm.StateToggleButtonContent = WindowState == WindowState.Normal ? FixedValues.MAXIMIZE_ICON : FixedValues.RESTORE_ICON;
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
                    if (e.OriginalSource is FrameworkElement originalSource &&
                        (originalSource.Name == "panel_historyButtons" || originalSource.Name == "updater" ||
                        (originalSource.Name == "panel_guideStepper" && App.GuidedTour.IsTourRunning)
                        ))
                    {
                        return;
                    }

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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Mediator.Instance.NotifyColleagues(ViewModelMessage.CreateSnapshot);
#else
            throw new Exception("Whoop whoop, I'm a crash!");
#endif
        }

        private void UndoButton_Loaded(object sender, RoutedEventArgs e)
        {
            undoButton = sender as Button;
            undoHistoryMenu = GenerateMenu(vm.History.UndoHistory.Stack, true);
            undoButton.ContextMenu = undoHistoryMenu;
        }

        private void RedoButton_Loaded(object sender, RoutedEventArgs e)
        {
            redoButton = sender as Button;
            redoHistoryMenu = GenerateMenu(vm.History.RedoHistory.Stack, false);
            redoButton.ContextMenu = redoHistoryMenu;
        }

        private ContextMenu GenerateMenu(IEnumerable<CabinHistoryEntry> history, bool isUndo)
        {
            ContextMenu contextMenu = new ContextMenu()
            {
                //MaxHeight = 300,
            };

            foreach (CabinHistoryEntry historyEntry in history)
            {
                contextMenu.Items.Add(GenerateHistoryItem(historyEntry, isUndo));
            }

            return contextMenu;
        }

        private MenuItem GenerateHistoryItem(CabinHistoryEntry historyEntry, bool isUndo)
        {
            ICommand command;
            if (isUndo)
            {
                command = vm.UndoUntilCommand;
            }
            else
            {
                command = vm.RedoUntilCommand;
            }

            return new MenuItem()
            {
                Header = historyEntry.Message,
                DataContext = historyEntry,
                Command = command,
                CommandParameter = historyEntry
            };
        }

        private void ManageBakedTemplates_Click(object sender, RoutedEventArgs e)
        {
            ManageBakedTemplatesDialog dialog = new ManageBakedTemplatesDialog();
            dialog.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.SessionStart = DateTime.Now;
            var test = App.Settings.UsageTime;
            if (!App.Settings.DonateMessageShown &&
                App.Settings.UsageTime.TotalHours >= FixedValues.HOURS_BEFORE_SUPPORT_NOTIFICATION)
            {
                ShowSupportNotification();
            }
        }

        private void ShowSupportNotification(int showDelay = 4000)
        {
            supportNotification = Notification.MakeNotification("Enjoying the editor?",
                "I hope you're enjoying working within the editor so far!\n\n" +
                "And if you're feeling extra generous today:", FixedValues.ICON_HEART,
                (Style)App.Current.FindResource("SupportButtonStyle"),
                showDelay, FixedValues.HEART_BRUSH);
            supportNotification.ButtonClicked += BuyACoffee_Clicked;
            supportNotification.Closed += SupportNotification_Closed;
        }

        private void BuyACoffee_Clicked(object sender, EventArgs e)
        {
            Process.Start("https://ko-fi.com/midnight_bagel");
            supportNotification?.Close();
        }

        private void SupportNotification_Closed(object sender, NotificationClosedEventArgs e)
        {
            supportNotification.ButtonClicked -= BuyACoffee_Clicked;
            supportNotification.Closed -= SupportNotification_Closed;
            supportNotification = null;
            App.Settings.DonateMessageShown = true;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            vm.ShowAbout();
        }

        private void BackTo_Click(object sender, RoutedEventArgs e)
        {
            if (vm.IsViewNotEditor)
            {
                vm.ReturnToEditor();
            }
            else
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessage.BackToLayoutOverview);
            }
        }

        private void DebugMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (vm.EditorViewModel != null && sender is MenuItem menuItem)
            {
                switch (menuItem.Tag.ToString())
                {
                    case "aircraft":
                        if (vm.EditorViewModel.SelectedLayoutSet == null)
                        {
                            return;
                        }

                        string aircraftTempPath = Path.Combine(App.ThumbnailsPath, vm.EditorViewModel.SelectedLayoutSet.AircraftName);
                        if (!Directory.Exists(aircraftTempPath))
                        {
                            return;
                        }

                        Process.Start(aircraftTempPath);
                        break;
                    case "layout":
                        if (vm.EditorViewModel.ActiveLayout == null)
                        {
                            return;
                        }

                        string layoutTempPath = Path.Combine(vm.EditorViewModel.ActiveLayout.ThumbnailDirectory);
                        if (!Directory.Exists(layoutTempPath))
                        {
                            return;
                        }

                        Process.Start(layoutTempPath);
                        break;
                    case "clear_log":
                        Logger.Default.ClearLog();
                        break;
                    case "open_log":
                        Process.Start(Logger.Default.FilePath);
                        break;
                    case "kofi":
                        ShowSupportNotification(0);
                        break;
                    case "toggle_log_output":
                        vm.ShowLogOutput = !vm.ShowLogOutput;
                        break;
                }
            }
        }

        private void LogOutput_ClosingRequested(object sender, EventArgs e)
        {
            vm.ShowLogOutput = false;
        }

        private void log_output_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Default.AttachedConsole = log_output;
        }
    }
}
