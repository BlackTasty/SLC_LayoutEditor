using Newtonsoft.Json.Linq;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Events;
using SLC_LayoutEditor.Core.Guide;
using SLC_LayoutEditor.ViewModel.Communication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Resources;
using System.Windows.Threading;
using Tasty.Logging;
using Tasty.ViewModel.Communication;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Renders a semi-transparent layer of color over the application, 
    /// but leaving out a circle area around the adorned element.
    /// Text can be displayed besides the cropped out area
    /// </summary>
    internal class LiveGuideAdorner : Adorner
    {
        public event EventHandler<LiveGuideClosedEventArgs> Closed;

        private const double BACKDROP_SAFEZONE_SIZE = 250;
        private const double STEPPER_BUTTON_SIZE = 12;
        private const double STEPPER_BUTTON_PADDING = 8;
        private const double GIF_CORNER_RADIUS = 16;

        private readonly double margin;
        private readonly double padding;
        private readonly double cornerRadius;
        private readonly double highlightCornerRadius;
        private readonly double safeZoneSize;

        private readonly double widthOffset;
        private readonly double heightOffset;
        private readonly double radiusOffset;
        private readonly double highlightXOffset;
        private readonly double highlightYOffset;
        private readonly double textAreaXOffset;
        private readonly double textAreaYOffset;

        private readonly double radius;
        private readonly string title;
        private readonly string description;
        private readonly Brush overlayBrush;
        private readonly GuideTextPosition textPosition;
        private readonly bool isCircularCutout;

        private readonly bool areTourStepsVisible;
        private readonly int currentTourStep;
        private readonly int totalTourSteps;
        private readonly string tourStepCategory;

        private readonly bool applyOverlayToAll;

        private Rect stepBackHitRect;

        private readonly Window window;
        private readonly UIElement guidedElement;

        private readonly bool areOverridesSet;

        private bool isMouseDown;
        private bool isStepBackMouseDown;


        private string gifFile;
        private double gifScaling;

        private GifBitmapDecoder gifDecoder;
        private DispatcherTimer gifRenderer;
        private int currentFrameIndex;
        private BitmapSource renderedFrame;
        Point gifPosition;
        Size gifSize;
        BitmapSource firstFrame;
        Rect renderRect;
        DrawingBrush maskBrush;

        private GuideTextPosition? checkedPosition;

        public UIElement GuidedElement => guidedElement;

        public static Adorner AttachAdorner(UIElement rootElement, UIElement guidedElement)
        {
            Adorner adorner = rootElement.AttachAdorner(typeof(LiveGuideAdorner), (UIElement)guidedElement, GuideAssist.GetMargin(guidedElement), GuideAssist.GetPadding(guidedElement),
                GuideAssist.GetCornerRadius(guidedElement), GuideAssist.GetHighlightCornerRadius(guidedElement), GuideAssist.GetTitle(guidedElement), GuideAssist.GetDescription(guidedElement), 
                FixedValues.LIVE_GUIDE_OVERLAY_BRUSH, Window.GetWindow(guidedElement), GuideAssist.GetIsCircularCutout(guidedElement), 
                GuideAssist.GetTextPosition(guidedElement), GuideAssist.GetTextAreaXOffset(guidedElement), GuideAssist.GetTextAreaYOffset(guidedElement), 
                GuideAssist.GetWidthOffset(guidedElement), GuideAssist.GetHeightOffset(guidedElement),
                GuideAssist.GetRadiusOffset(guidedElement), GuideAssist.GetHighlightXOffset(guidedElement), GuideAssist.GetHighlightYOffset(guidedElement),
                GuideAssist.GetSafeZoneSize(guidedElement), GuideAssist.GetOverrides(guidedElement));

            return adorner;
        }

        /// <summary>
        /// Attach the adorner to an element.
        /// </summary>
        public LiveGuideAdorner(UIElement rootElement, UIElement guidedElement, double margin, double padding
            , double cornerRadius, double highlightCornerRadius, string title, string description, 
            Brush overlayBrush, Window window, bool isCircularCutout, GuideTextPosition textPosition,
            double textAreaXOffset, double textAreaYOffset, double widthOffset, double heightOffset, double radiusOffset, 
            double highlightXOffset, double highlightYOffset, double safeZoneSize, GuideAssistOverrides overrides) : base(rootElement)
        {
            this.guidedElement = guidedElement;
            areOverridesSet = overrides?.AreOverridesSet ?? false;
            areTourStepsVisible = overrides?.AreTourStepsSet ?? false;
            applyOverlayToAll = overrides?.ApplyOverlayToAll ?? false;

            if (areTourStepsVisible)
            {
                currentTourStep = overrides.CurrentTourStep.HasValue ? overrides.CurrentTourStep.Value : 0;
                totalTourSteps = overrides.TotalTourSteps.HasValue ? overrides.TotalTourSteps.Value : 0;
                tourStepCategory = overrides.TourStepCategory;
            }

            if (overrides?.GIFName != null)
            {
                gifFile = "Resources/Guides/" + overrides.GIFName;
                gifScaling = overrides.GIFScaling;

                StreamResourceInfo streamInfo = Application.GetResourceStream(new Uri(gifFile, UriKind.Relative));
                if (streamInfo == null)
                {
                    Logger.Default.WriteLog($"Resource '{gifFile}' not found.");
                }
                else
                {
                    gifDecoder = new GifBitmapDecoder(streamInfo.Stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    gifRenderer = new DispatcherTimer(DispatcherPriority.Render);
                    gifRenderer.Interval = TimeSpan.FromMilliseconds(16);
                    gifRenderer.Tick += GifRenderer_Tick;
                    gifRenderer.Start();

                    firstFrame = gifDecoder.Frames[0];
                    gifSize = new Size(firstFrame.Width * gifScaling, firstFrame.Height * gifScaling);
                    renderRect = new Rect(0, 0, firstFrame.Width, firstFrame.Height);

                    maskBrush = new DrawingBrush(new GeometryDrawing(Brushes.Blue, null, 
                        new RectangleGeometry(renderRect, GIF_CORNER_RADIUS, GIF_CORNER_RADIUS)));
                }
            }

            this.radiusOffset = !overrides?.RadiusOffset.HasValue ?? true ? radiusOffset : overrides.RadiusOffset.Value;
            this.widthOffset = !overrides?.WidthOffset.HasValue ?? true ? widthOffset : overrides.WidthOffset.Value;
            this.heightOffset = !overrides?.HeightOffset.HasValue ?? true ? heightOffset : overrides.HeightOffset.Value;
            this.highlightXOffset = !overrides?.HighlightXOffset.HasValue ?? true ? highlightXOffset : overrides.HighlightXOffset.Value;
            this.highlightYOffset = !overrides?.HighlightYOffset.HasValue ?? true ? highlightYOffset : overrides.HighlightYOffset.Value;
            this.textAreaXOffset = !overrides?.TextAreaXOffset.HasValue ?? true ? textAreaXOffset : overrides.TextAreaXOffset.Value;
            this.textAreaYOffset = !overrides?.TextAreaYOffset.HasValue ?? true ? textAreaYOffset : overrides.TextAreaYOffset.Value;
            this.safeZoneSize = !overrides?.SafeZoneSize.HasValue ?? true ? safeZoneSize : overrides.SafeZoneSize.Value;

            this.isCircularCutout = !overrides?.IsCircularCutout.HasValue ?? true ? isCircularCutout : overrides.IsCircularCutout.Value;
            this.margin = !overrides?.Margin.HasValue ?? true ? margin : overrides.Margin.Value;
            this.padding = !overrides?.Padding.HasValue ?? true ? padding : overrides.Padding.Value;
            this.cornerRadius = !overrides?.CornerRadius.HasValue ?? true ? cornerRadius : overrides.CornerRadius.Value;
            this.highlightCornerRadius = !overrides?.HighlightCornerRadius.HasValue ?? true ? highlightCornerRadius : overrides.HighlightCornerRadius.Value;
            this.radius = GetRadius(guidedElement, margin);
            this.title = overrides?.Title == null ? title : overrides.Title;
            this.description = overrides?.Description == null ? description : overrides.Description;
            this.textPosition = overrides?.TextPosition== null ? textPosition : overrides.TextPosition.Value;
            this.overlayBrush = overlayBrush;

            this.window = window;
            PreviewMouseDown += LiveGuideAdorner_PreviewMouseDown;
            PreviewMouseUp += LiveGuideAdorner_PreviewMouseUp;
            ClipToBounds = false;
        }

        private void GifRenderer_Tick(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void LiveGuideAdorner_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isMouseDown = true;

            if (areTourStepsVisible && stepBackHitRect.Contains(e.GetPosition(this)))
            {
                isStepBackMouseDown = true;
            }
        }

        private void LiveGuideAdorner_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                RemoveOverrides();
                AdornedElement.RemoveAdorner(this);
                Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerClosed);
                OnClosed(!isStepBackMouseDown ? new LiveGuideClosedEventArgs() : new LiveGuideClosedEventArgs(-1));
            }
        }

        private void RemoveOverrides()
        {
            if (areOverridesSet)
            {
                GuideAssist.SetOverrides(guidedElement, null);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Point relativePosition = guidedElement.TransformToAncestor(AdornedElement).Transform(new Point(0, 0));
            Rect adornedElementRect = new Rect(relativePosition, guidedElement.RenderSize);
            adornedElementRect.X += highlightXOffset - widthOffset / 2;
            adornedElementRect.Y += highlightYOffset - heightOffset / 2;

            adornedElementRect.Width += widthOffset;
            adornedElementRect.Height += heightOffset;

            double drawingRectX = adornedElementRect.X < 0 ? adornedElementRect.X * 2 - adornedElementRect.Width / 2 : 0;
            double drawingRectY = adornedElementRect.Y < 12 ? adornedElementRect.Y - 12 - adornedElementRect.Height / 2 : 0;

            Point adornedElementCenter = new Point(adornedElementRect.Width / 2, adornedElementRect.Height / 2);
            //adornedElementCenter.Offset(highlightXOffset, highlightYOffset);
            Rect drawingRect = new Rect(-BACKDROP_SAFEZONE_SIZE, -BACKDROP_SAFEZONE_SIZE,
                AdornedElement.RenderSize.Width + BACKDROP_SAFEZONE_SIZE, AdornedElement.RenderSize.Height + BACKDROP_SAFEZONE_SIZE);

            // Generate texts
            FormattedText formattedTitle = GetFormattedText(title, FontWeights.Bold, 32, FixedValues.DEFAULT_BRUSH);
            FormattedText formattedDescription = GetFormattedText(description, FontWeights.Thin, 16, FixedValues.DEFAULT_SECONDARY_BRUSH);

            string closeInfoText = !areTourStepsVisible ? "Click anywhere to close this info" :
                !string.IsNullOrWhiteSpace(tourStepCategory) ? 
                string.Format("Guided tour - {0} ({1}/{2})", tourStepCategory, currentTourStep, totalTourSteps) :
                string.Format("Guided tour ({0}/{1})", currentTourStep, totalTourSteps);
            FormattedText formattedCloseInfo = GetFormattedText(closeInfoText, 
                FontWeights.Thin, !areTourStepsVisible ? 10 : 12, FixedValues.YELLOW_BRUSH);

            Rect textAreaRect = GetTextAreaRect(textPosition, adornedElementRect, adornedElementCenter, formattedTitle, formattedDescription, formattedCloseInfo);

            Point titlePosition = textAreaRect.Location.MakeOffset(padding * 2, 8);
            Point descriptionPosition = titlePosition.MakeOffset(0, formattedTitle.Height + 8);
            Point closeInfoPosition = descriptionPosition.MakeOffset(0, formattedDescription.Height + 12);

            #region Prepare GIF animation frame if set
            if (gifDecoder != null)
            {
                double textAreaGifWidthOffset = Math.Max(gifSize.Width + 2 - textAreaRect.Width, 0);
                double textAreaGifXOffset = textAreaGifWidthOffset / 2;
                if (textAreaGifWidthOffset > 0)
                {
                    textAreaRect.Width += textAreaGifWidthOffset;
                    textAreaRect.X -= textAreaGifXOffset;
                }

                double textAreaOffset = gifSize.Height / 2 + 8;
                textAreaRect.Y -= textAreaOffset;
                titlePosition.Offset(-textAreaGifXOffset, -textAreaOffset);
                descriptionPosition.Offset(-textAreaGifXOffset, -textAreaOffset);
                closeInfoPosition.Offset(-textAreaGifXOffset, gifSize.Height + 8 - textAreaOffset);

                textAreaRect.Height += gifSize.Height + 8 * 2;

                gifPosition = new Point((textAreaRect.Right - textAreaRect.Width / 2 - gifSize.Width / 2) / gifScaling,
                    (formattedDescription.Height + descriptionPosition.Y + 8) / gifScaling);

                DrawingVisual drawingImage = new DrawingVisual();

                using (DrawingContext renderContext = drawingImage.RenderOpen())
                {
                    Rect newRenderRect = currentFrameIndex != 0 ? new Rect(0, 0, renderedFrame.Width, renderedFrame.Height) :
                         new Rect(0, 0, firstFrame.Width, firstFrame.Height);

                    if (renderRect != newRenderRect)
                    {
                        renderRect = newRenderRect;
                        maskBrush = new DrawingBrush(new GeometryDrawing(Brushes.Blue, null, 
                            new RectangleGeometry(renderRect, GIF_CORNER_RADIUS, GIF_CORNER_RADIUS)));
                    }

                    if (currentFrameIndex != 0)
                    {
                        renderContext.DrawImage(renderedFrame, renderRect);

                        BitmapSource dynamicFrame = gifDecoder.Frames[currentFrameIndex];
                        Point frameOffset = GetFrameOffset((BitmapMetadata)dynamicFrame.Metadata);
                        //renderContext.PushTransform(new ScaleTransform(gifScaling, gifScaling));
                        renderContext.DrawImage(dynamicFrame,
                            new Rect(frameOffset, new Size(dynamicFrame.Width, dynamicFrame.Height)));
                    }
                    else
                    {
                        renderContext.PushOpacityMask(maskBrush);
                        renderContext.DrawImage(firstFrame, renderRect);
                    }
                }

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)drawingRect.Width, (int)drawingRect.Height,
                    96, 96, PixelFormats.Pbgra32);
                

                renderBitmap.Render(drawingImage);
                renderBitmap.Freeze();
                renderedFrame = renderBitmap;

                gifRenderer.Interval = TimeSpan.FromMilliseconds(double.Parse(((BitmapMetadata)gifDecoder.Frames[currentFrameIndex].Metadata)
                    .GetQuery("/grctlext/Delay").ToString()) * 4);
                // Update the frame index for the next rendering
                currentFrameIndex = (currentFrameIndex + 1) % gifDecoder.Frames.Count;
            }
            #endregion

            if (areOverridesSet)
            {
                closeInfoPosition = Util.GetChildCenterPosition(textAreaRect, new Rect(
                        closeInfoPosition, 
                        new Size(formattedCloseInfo.Width, formattedCloseInfo.Height)
                    ), true, false);
            }

            if (!applyOverlayToAll)
            {
                ApplyOpacityMask(drawingContext, drawingRect, adornedElementRect, adornedElementCenter, textAreaRect);
            }

            drawingContext.DrawRectangle(overlayBrush, null, drawingRect);

            drawingContext.DrawRoundedRectangle(FixedValues.LIVE_GUIDE_TEXT_BACK_BRUSH, 
                new Pen(FixedValues.LIVE_GUIDE_TEXT_BORDER_BRUSH, 1.5), 
                textAreaRect, cornerRadius, cornerRadius);

            // Render GIF frame if available
            if (renderedFrame != null)
            {
                // Apply a ScaleTransform to the DrawingContext
                drawingContext.PushTransform(new ScaleTransform(gifScaling, gifScaling));

                drawingContext.DrawImage(renderedFrame, new Rect(gifPosition, new Size(renderedFrame.Width, renderedFrame.Height)));

                // Pop the ScaleTransform to avoid affecting subsequent drawings
                drawingContext.Pop();
            }

            // Render texts onto adorner
            drawingContext.DrawText(formattedTitle, titlePosition);
            drawingContext.DrawText(formattedDescription, descriptionPosition);
            drawingContext.DrawText(formattedCloseInfo, closeInfoPosition);

            base.OnRender(drawingContext);
        }

        private Point GetFrameOffset(BitmapMetadata metadata)
        {
            object xQuery = metadata.GetQuery("/imgdesc/Left");
            object yQuery = metadata.GetQuery("/imgdesc/Top");

            if (double.TryParse(xQuery.ToString(), out double x) && double.TryParse(yQuery.ToString(), out double y))
            {
                return new Point(x, y);
            }

            return new Point();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size result = base.MeasureOverride(constraint);
            // ... add custom measure code here if desired ...
            InvalidateVisual();

            return result;
        }

        private void RenderButton(DrawingContext context, Rect iconRect, StreamGeometry icon)
        {
            TranslateTransform translation = new TranslateTransform(iconRect.X, iconRect.Y);
            context.PushTransform(translation);
            context.DrawGeometry(FixedValues.DEFAULT_BRUSH, null, icon);
            context.Pop();
        }

        private void ApplyOpacityMask(DrawingContext context, Rect drawingRect, Rect adornedElementRect, Point adornedElementCenter,
            Rect textAreaRect)
        {
            RectangleGeometry backdropGeometry = new RectangleGeometry(drawingRect, 8, 8);

            CombinedGeometry geometry;
            if (isCircularCutout)
            {
                EllipseGeometry highlightGeometry = new EllipseGeometry(adornedElementRect)
                {
                    RadiusX = radius,
                    RadiusY = radius,
                };
                geometry = new CombinedGeometry(GeometryCombineMode.Xor,
                   backdropGeometry,
                   highlightGeometry);
            }
            else
            {
                double diameter = radiusOffset / 2;
                Rect adjustedHighlightRect = new Rect(adornedElementRect.Location.X - diameter, adornedElementRect.Location.Y - diameter,
                    adornedElementRect.Width + radiusOffset, adornedElementRect.Height + radiusOffset);

                if (textPosition != GuideTextPosition.Over)
                {
                    geometry = new CombinedGeometry(GeometryCombineMode.Xor,
                       backdropGeometry,
                       new RectangleGeometry(adjustedHighlightRect, highlightCornerRadius, highlightCornerRadius));
                }
                else
                {
                    Rect adjustedTextAreaRect = new Rect(textAreaRect.X - margin / 2, textAreaRect.Y - margin / 2,
                        textAreaRect.Width + margin, textAreaRect.Height + margin);
                    geometry = new CombinedGeometry(GeometryCombineMode.Union,
                        new CombinedGeometry(GeometryCombineMode.Xor,
                           backdropGeometry,
                           new RectangleGeometry(adjustedHighlightRect, highlightCornerRadius, highlightCornerRadius)),
                        new RectangleGeometry(adjustedTextAreaRect, highlightCornerRadius, highlightCornerRadius));
                }
            }

            DrawingBrush mask = new DrawingBrush(new GeometryDrawing(Brushes.Blue, null, geometry));
            context.PushOpacityMask(mask);
        }

        private double GetRadius(UIElement adornedElement, double margin)
        {
            double radius = (adornedElement.DesiredSize.Width > adornedElement.DesiredSize.Height ?
                adornedElement.DesiredSize.Width : adornedElement.DesiredSize.Height) + 8;
            return (isCircularCutout ? radius : radius / 4) + radiusOffset;
        }

        private FormattedText GetFormattedText(string text, FontWeight fontWeight, double fontSize, Brush brush)
        {
            return new FormattedText(text, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
                new Typeface(FixedValues.FONT_FAMILY, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                fontSize, brush);
        }

        private Rect GetTextAreaRect(GuideTextPosition textPosition, Rect adornedElementRect, Point adornedElementCenter, 
            FormattedText formattedTitle, FormattedText formattedDescription, FormattedText formattedCloseInfo, bool skipBoundsCheck = false)
        {
            double windowWidth = AdornedElement.RenderSize.Width;
            double windowHeight = AdornedElement.RenderSize.Height;

            double offset = (isCircularCutout ? radius + margin : margin) - padding;
            Point titlePosition = new Point(16, 16);

            double textAreaWidth = Math.Max(formattedTitle.Width, formattedDescription.Width) + 16 + padding * 2;

            double textAreaHeight = formattedTitle.Height + formattedDescription.Height + formattedCloseInfo.Height + padding * 5;

            double textAreaCenterX = textAreaWidth / 2 + highlightXOffset;
            double textAreaCenterY = textAreaHeight / 2 + highlightYOffset;

            Size textAreaSize = new Size(textAreaWidth, textAreaHeight);

            switch (textPosition)
            {
                case GuideTextPosition.Right:
                    titlePosition = new Point(adornedElementRect.Right + offset, adornedElementRect.Y + adornedElementCenter.Y - textAreaCenterY);
                    break;
                case GuideTextPosition.Bottom:
                    titlePosition = new Point(adornedElementRect.X + adornedElementCenter.X - textAreaCenterX, adornedElementRect.Bottom + offset);
                    break;
                case GuideTextPosition.Left:
                    titlePosition = new Point(adornedElementRect.Left - textAreaWidth - offset - 16, adornedElementRect.Bottom - offset + margin + textAreaCenterY);
                    break;
                case GuideTextPosition.Top:
                    titlePosition = new Point(adornedElementRect.X + adornedElementCenter.X - textAreaCenterX, adornedElementRect.Top - offset - textAreaHeight);
                    break;
                case GuideTextPosition.Over:
                    titlePosition = new Point(adornedElementRect.X + adornedElementCenter.X - textAreaCenterX, adornedElementRect.Y + adornedElementCenter.Y - textAreaCenterY);
                    break;
                default: // Calculate the optimal position automatically

                    foreach (GuideTextPosition possiblePosition in Enum.GetValues(typeof(GuideTextPosition)))
                    {
                        if (possiblePosition == GuideTextPosition.Auto || possiblePosition == GuideTextPosition.Over || 
                            (checkedPosition.HasValue && checkedPosition.Value == possiblePosition))
                        {
                            continue;
                        }

                        Rect possibleRect = GetTextAreaRect(possiblePosition, adornedElementRect, adornedElementCenter, 
                            formattedTitle, formattedDescription, formattedCloseInfo, true);

                        if (IsTextAreaInBounds(possiblePosition, possibleRect, windowWidth, windowHeight))
                        {
                            return possibleRect;
                        }
                    }
                    break;
            }

            //titlePosition.Offset(-padding, -padding);

            Rect textAreaRect = new Rect(titlePosition, textAreaSize);
            textAreaRect.Offset(textAreaXOffset, textAreaYOffset);


            double totalRightOffset = textAreaRect.Right;
            if (totalRightOffset > AdornedElement.RenderSize.Width)
            {
                textAreaRect.Offset(AdornedElement.RenderSize.Width - totalRightOffset - safeZoneSize, 0);
            }

            double totalTopOffset = textAreaRect.Top;
            if (totalTopOffset < 0)
            {
                textAreaRect.Offset(0, safeZoneSize + Math.Abs(totalTopOffset));
            }

            double totalLeftOffset = textAreaRect.Left;
            if (totalLeftOffset < 0)
            {
                textAreaRect.Offset(Math.Abs(totalLeftOffset) + safeZoneSize, 0);
            }

            double totalBottomOffset = textAreaRect.Bottom;
            if (totalBottomOffset > AdornedElement.RenderSize.Height)
            {
                textAreaRect.Offset(0, AdornedElement.RenderSize.Height - totalBottomOffset - safeZoneSize);
            }

            /*if (!skipBoundsCheck && !IsTextAreaInBounds(textPosition, textAreaRect, windowWidth, windowHeight))
            {
                checkedPosition = textPosition;
                return GetTextAreaRect(GuideTextPosition.Auto, adornedElementRect, adornedElementCenter, formattedTitle, formattedDescription, formattedCloseInfo, true);
            }*/

            return textAreaRect;
        }

        private bool IsTextAreaInBounds(GuideTextPosition textPosition, Rect textAreaRect, double windowWidth, double windowHeight, bool includeSafeZone = true)
        {
            double left = includeSafeZone ? textAreaRect.Left - safeZoneSize : textAreaRect.Left;
            double top = includeSafeZone ? textAreaRect.Top - safeZoneSize : textAreaRect.Top;
            double right = includeSafeZone ? textAreaRect.Right + safeZoneSize : textAreaRect.Right;
            double bottom = includeSafeZone ? textAreaRect.Bottom + safeZoneSize : textAreaRect.Bottom;

            switch (textPosition)
            {
                case GuideTextPosition.Right:
                    return right <= windowWidth && top >= 0 && bottom <= windowHeight;
                case GuideTextPosition.Bottom:
                    return right <= windowWidth && left >= 0 && bottom <= windowHeight;
                case GuideTextPosition.Left:
                    return left >= 0 && top >= 0 && bottom <= windowHeight;
                case GuideTextPosition.Top:
                    return bottom <= windowWidth && left >= 0 && top >= 0;
            }

            return false;
        }

        protected virtual void OnClosed(LiveGuideClosedEventArgs e)
        {
            if (gifRenderer != null)
            {
                gifRenderer.Tick -= GifRenderer_Tick;
                gifRenderer.Stop();
            }
            Closed?.Invoke(this, e);
        }
    }
}
