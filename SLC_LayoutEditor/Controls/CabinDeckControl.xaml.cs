﻿using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Cabin.Renderer;
using SLC_LayoutEditor.Core.Dialogs;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Memento;
using SLC_LayoutEditor.UI.Dialogs;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using System.Xml.Linq;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for DeckLayoutControl.xaml
    /// </summary>
    public partial class CabinDeckControl : StackPanel, IDisposable
    {
        public event EventHandler<SelectedSlotsChangedEventArgs> SelectedSlotsChanged;

        public event EventHandler<RemoveCabinDeckEventArgs> RemoveDeckClicked;
        public event EventHandler<EventArgs> LayoutRegenerated;
        public event EventHandler<EventArgs> LayoutLoading;

        public event EventHandler<EventArgs> DeckRendered;

        public event EventHandler<EventArgs> RenderSizeChanged;

        private Point dragStartPosition;
        private Rectangle selectionBox;
        private bool isMouseDown;

        private Adorner deckTitleAdorner;

        private CabinDeckRenderer renderer;
        private DragSelectRenderer selectRenderer;
        private ToolTip tooltip;

        #region CabinDeck property
        public CabinDeck CabinDeck
        {
            get { return (CabinDeck)GetValue(CabinDeckProperty); }
            set { SetValue(CabinDeckProperty, value); }
        }

        private static FrameworkPropertyMetadata cabinDeckMetadata =
                new FrameworkPropertyMetadata(
                    null,     // Default data
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.Journal,
                    null,    // Property changed callback
                    null);   // Coerce data callback

        // Using a DependencyProperty as the backing store for CabinDeck.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CabinDeckProperty =
            DependencyProperty.Register("CabinDeck", typeof(CabinDeck), typeof(CabinDeckControl), cabinDeckMetadata);

        private static void OnCabinDeckChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is CabinDeckControl control)
            {
                if (control.renderer != null)
                {
                    control.renderer.SetCabinDeck(control.CabinDeck);
                }
            }
        }
        #endregion

        public ContextMenu GuideMenu
        {
            get { return (ContextMenu)GetValue(GuideMenuProperty); }
            set { SetValue(GuideMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GuideMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GuideMenuProperty =
            DependencyProperty.Register("GuideMenu", typeof(ContextMenu), typeof(CabinDeckControl), new PropertyMetadata(null));

        public bool HasAnyIssues => CabinDeck?.HasAnyIssues ?? false;

        public bool HasMinorIssues => CabinDeck?.HasMinorIssues ?? false;

        public bool HasSevereIssues => CabinDeck?.HasSevereIssues?? false;

        public CabinDeckControl()
        {
            InitializeComponent();

            Mediator.Instance.Register(o =>
            {
                if (o is CabinDeck cabinDeck && cabinDeck == CabinDeck)
                {
                    renderer?.SelectAllSlots();
                }
            }, ViewModelMessage.Keybind_SelectAllSlotsOnDeck);
        }

        public void RefreshCabinDeckLayout()
        {
            Logger.Default.WriteLog("Re-rendering cabin deck...");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            deck_view.Dispatcher.Invoke(() =>
            {
                if (renderer != null)
                {
                    renderer.RenderCabinDeck();
                }
                else
                {
                    renderer = new CabinDeckRenderer(CabinDeck);
                    renderer.ChangeTooltip += Renderer_ChangeTooltip;
                    renderer.CloseTooltip += Renderer_CloseTooltip;
                    renderer.SelectedSlotsChanged += Renderer_SelectedSlotsChanged;
                    renderer.SizeChanged += Renderer_SizeChanged;

                    selectRenderer = new DragSelectRenderer(renderer);
                }

                deck_view.Source = renderer.Output;
                OnDeckRendered(EventArgs.Empty);
            });

            sw.Stop();
            Logger.Default.WriteLog("Cabin deck rendered in {0} seconds", Math.Round((double)sw.ElapsedMilliseconds / 1000, 3));
        }

        private void Renderer_CloseTooltip(object sender, EventArgs e)
        {
            if (tooltip != null)
            {
                tooltip.IsOpen = false;
            }
        }

        private void Renderer_SizeChanged(object sender, CabinDeckSizeChangedEventArgs e)
        {
            deck_view.Source = renderer.Output;

            if (e.CreateHistoryStep)
            {
                CabinHistory.Instance.RecordChanges(e);
            }
            OnRenderSizeChanged(e);
        }

        private void Renderer_SelectedSlotsChanged(object sender, SelectedSlotsChangedEventArgs e)
        {
            OnSelectedSlotsChanged(new SelectedSlotsChangedEventArgs(e, this, CabinDeck.Floor));
        }

        private void Renderer_ChangeTooltip(object sender, EventArgs e)
        {
            if (tooltip == null)
            {
                tooltip = new ToolTip()
                {
                    Content = renderer.Tooltip,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    PlacementTarget = deck_view
                };
            }
            else
            {
                tooltip.Content = renderer.Tooltip;
            }

            tooltip.IsOpen = deck_view.IsMouseDirectlyOver && renderer.Tooltip != null;
        }

        public void DeselectSlots()
        {
            renderer.DeselectAllSlots();
        }

        public void SelectSlots(IEnumerable<CabinSlot> cabinSlots)
        {
            renderer.SelectSlots(cabinSlots);
        }

        public bool GenerateThumbnailForDeck(string thumbnailPath, bool overwrite = false)
        {
            return renderer?.GenerateThumbnail(overwrite) ?? false;
        }

        public void RenderAdorners()
        {
            if (deckTitleAdorner != null)
            {
                card_deckTitle.RemoveAdorner(deckTitleAdorner);
            }
            deckTitleAdorner = card_deckTitle.AttachAdorner(typeof(CabinDeckCardAdorner));
        }

        public void RedrawDirtySlots()
        {
            renderer?.RedrawDirtySlots();
        }

        public void Dispose()
        {
            if (CabinDeck != null)
            {
                CabinDeck.DeckSlotLayoutChanged -= CabinDeck_DeckSlotLayoutChanged;
                if (tooltip != null)
                {
                    tooltip.IsOpen = false;
                    tooltip = null;
                }
            }
        }

        private BackgroundWorker layoutLoader;

        private void container_Loaded(object sender, RoutedEventArgs e)
        {
            OnLayoutLoading(EventArgs.Empty);
            layoutLoader = new BackgroundWorker();
            layoutLoader.DoWork += LayoutLoader_DoWork;
            layoutLoader.RunWorkerCompleted += LayoutLoader_RunWorkerCompleted;

            layoutLoader.RunWorkerAsync();

            RenderAdorners();
        }

        private void LayoutLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            RefreshCabinDeckLayout();
        }

        private void LayoutLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CabinDeck != null)
            {
                CabinDeck.DeckSlotLayoutChanged += CabinDeck_DeckSlotLayoutChanged;
            }

            layoutLoader.DoWork -= LayoutLoader_DoWork;
            layoutLoader.RunWorkerCompleted -= LayoutLoader_RunWorkerCompleted;
            OnLayoutRegenerated(EventArgs.Empty);
        }

        private void CabinDeck_DeckSlotLayoutChanged(object sender, EventArgs e)
        {
            RefreshCabinDeckLayout();
        }

        private void RemoveDeck_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveDeckClicked(new RemoveCabinDeckEventArgs(CabinDeck));
        }

        protected virtual void OnLayoutRegenerated(EventArgs e)
        {
            LayoutRegenerated?.Invoke(this, e);
        }

        protected virtual void OnLayoutLoading(EventArgs e)
        {
            LayoutLoading?.Invoke(this, e);
        }

        protected virtual void OnRemoveDeckClicked(RemoveCabinDeckEventArgs e)
        {
            RemoveDeckClicked?.Invoke(this, e);
        }

        protected virtual void OnRenderSizeChanged(EventArgs e)
        {
            RenderSizeChanged?.Invoke(this, e);
        }

        protected virtual void OnDeckRendered(EventArgs e)
        {
            DeckRendered?.Invoke(this, e);
        }

        private void deck_view_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            deck_dragselect.Source = selectRenderer.RefreshDragSelect(Mouse.GetPosition(deck_view));
            renderer?.CheckMouseOver(deck_view);

        }

        private void deck_view_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            deck_dragselect.Source = selectRenderer.StartDragSelect(Mouse.GetPosition(deck_view));
            renderer?.CheckMouseClick(deck_view, true);
        }

        private void deck_view_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            deck_dragselect.Source = selectRenderer.StopDragSelect();
            renderer?.CheckMouseClick(deck_view, false);
        }

        private void deck_view_MouseLeave(object sender, MouseEventArgs e)
        {
            deck_dragselect.Source = selectRenderer.StopDragSelect();
            renderer?.CheckMouseClick(deck_view, false);
            renderer?.CheckMouseOver(deck_view);
        }

        protected virtual void OnSelectedSlotsChanged(SelectedSlotsChangedEventArgs e)
        {
            SelectedSlotsChanged?.Invoke(this, e);
        }
    }
}
