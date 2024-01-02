using Newtonsoft.Json.Linq;
using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Enum;
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
using System.Windows.Media;
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
        public event EventHandler<EventArgs> Closed;

        private const double BACKDROP_SAFEZONE_SIZE = 250;

        private readonly double margin;
        private readonly double padding;
        private readonly double cornerRadius;
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
        private readonly bool applyOverlayToAll;

        private readonly Window window;
        private readonly UIElement guidedElement;

        private readonly bool areOverridesSet;

        public UIElement GuidedElement => guidedElement;

        public static Adorner AttachAdorner(UIElement rootElement, UIElement guidedElement)
        {
            Adorner adorner = rootElement.AttachAdorner(typeof(LiveGuideAdorner), (UIElement)guidedElement, GuideAssist.GetMargin(guidedElement), GuideAssist.GetPadding(guidedElement),
                GuideAssist.GetCornerRadius(guidedElement), GuideAssist.GetTitle(guidedElement), GuideAssist.GetDescription(guidedElement), 
                FixedValues.LIVE_GUIDE_OVERLAY_BRUSH, Window.GetWindow(guidedElement), GuideAssist.GetIsCircleCutout(guidedElement), 
                GuideAssist.GetTextPosition(guidedElement), GuideAssist.GetTextAreaXOffset(guidedElement), GuideAssist.GetTextAreaYOffset(guidedElement), 
                GuideAssist.GetWidthOffset(guidedElement), GuideAssist.GetHeightOffset(guidedElement),
                GuideAssist.GetRadiusOffset(guidedElement), GuideAssist.GetHighlightXOffset(guidedElement), GuideAssist.GetHighlightYOffset(guidedElement),
                GuideAssist.GetSafeZoneSize(guidedElement), GuideAssist.GetOverrides(guidedElement));

            return adorner;
        }

        /// <summary>
        /// Attach the adorner to an element.
        /// </summary>
        /// <param name="rootElement">The element to attach the adorner to</param>
        /// <param name="margin">The margin between the circle around the adorned element and the element itself</param>
        /// <param name="title">The title for this guide</param>
        /// <param name="description">A short description describing the action</param>
        /// <param name="overlayBrush">The color of the overlay blending out other elements</param>
        /// <param name="windowSize">The size of the window</param>
        public LiveGuideAdorner(UIElement rootElement, UIElement guidedElement, double margin, double padding, double cornerRadius, string title, string description, 
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
            this.radius = GetRadius(guidedElement, margin);
            this.title = overrides?.Title == null ? title : overrides.Title;
            this.description = overrides?.Description == null ? description : overrides.Description;
            this.textPosition = overrides?.TextPosition== null ? textPosition : overrides.TextPosition.Value;
            this.overlayBrush = overlayBrush;

            this.window = window;
            PreviewMouseUp += LiveGuideAdorner_PreviewMouseUp;
            ClipToBounds = false;
        }

        private void LiveGuideAdorner_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RemoveOverrides();
            AdornedElement.RemoveAdorner(this);
            Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerClosed);
            OnClosed(EventArgs.Empty);
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
            adornedElementRect.X += highlightXOffset;
            adornedElementRect.Y += highlightYOffset;

            adornedElementRect.Width += widthOffset;
            adornedElementRect.Height += heightOffset;

            Point adornedElementCenter = new Point(adornedElementRect.Width / 2, adornedElementRect.Height / 2);
            //adornedElementCenter.Offset(highlightXOffset, highlightYOffset);
            Rect drawingRect = new Rect(0, 0,
                AdornedElement.RenderSize.Width + BACKDROP_SAFEZONE_SIZE, AdornedElement.RenderSize.Height + BACKDROP_SAFEZONE_SIZE);

            // Generate texts
            FormattedText formattedTitle = GetFormattedText(title, FontWeights.Bold, 32, FixedValues.DEFAULT_BRUSH);
            FormattedText formattedDescription = GetFormattedText(description, FontWeights.Thin, 16, FixedValues.DEFAULT_SECONDARY_BRUSH);
            FormattedText formattedCloseInfo = GetFormattedText(!areTourStepsVisible ? "Click anywhere to close this info" :
                string.Format("Guided tour - {0}/{1}", currentTourStep, totalTourSteps), FontWeights.Thin, !areTourStepsVisible ? 10 : 12, FixedValues.YELLOW_BRUSH);

            Rect textAreaRect = GetTextAreaRect(textPosition, adornedElementRect, adornedElementCenter, formattedTitle, formattedDescription, formattedCloseInfo);

            Point titlePosition = textAreaRect.Location.MakeOffset(padding * 2, 8);
            Point descriptionPosition = titlePosition.MakeOffset(0, formattedTitle.Height + 8);
            Point closeInfoPosition = descriptionPosition.MakeOffset(0, formattedDescription.Height + 12);

            if (areOverridesSet)
            {
                closeInfoPosition = GetChildCenterPosition(textAreaRect, new Rect(
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

            // Render texts onto adorner
            drawingContext.DrawText(formattedTitle, titlePosition);
            drawingContext.DrawText(formattedDescription, descriptionPosition);
            drawingContext.DrawText(formattedCloseInfo, closeInfoPosition);

            base.OnRender(drawingContext);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var result = base.MeasureOverride(constraint);
            // ... add custom measure code here if desired ...
            InvalidateVisual();
            return result;
        }

        private void ApplyOpacityMask(DrawingContext context, Rect drawingRect, Rect adornedElementRect, Point adornedElementCenter,
            Rect textAreaRect)
        {
            CombinedGeometry geometry;
            if (isCircularCutout)
            {
                EllipseGeometry highlightGeometry = new EllipseGeometry(adornedElementRect)
                {
                    RadiusX = radius,
                    RadiusY = radius,
                };
                geometry = new CombinedGeometry(GeometryCombineMode.Xor,
                   new RectangleGeometry(drawingRect),
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
                       new RectangleGeometry(drawingRect),
                       new RectangleGeometry(adjustedHighlightRect, cornerRadius, cornerRadius));
                }
                else
                {
                    Rect adjustedTextAreaRect = new Rect(textAreaRect.X - margin / 2, textAreaRect.Y - margin / 2,
                        textAreaRect.Width + margin, textAreaRect.Height + margin);
                    geometry = new CombinedGeometry(GeometryCombineMode.Union,
                        new CombinedGeometry(GeometryCombineMode.Xor,
                           new RectangleGeometry(drawingRect),
                           new RectangleGeometry(adjustedHighlightRect, cornerRadius, cornerRadius)),
                        new RectangleGeometry(adjustedTextAreaRect, cornerRadius, cornerRadius));
                }
            }

            DrawingBrush mask = new DrawingBrush(new GeometryDrawing(Brushes.Blue, null, geometry));
            context.PushOpacityMask(mask);
            AddLogicalChild(new Button());
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
            FormattedText formattedTitle, FormattedText formattedDescription, FormattedText formattedCloseInfo)
        {
            double offset = (isCircularCutout ? radius + margin : margin) - padding;
            Point titlePosition = new Point(16, 16);

            double textAreaWidth = Math.Max(formattedTitle.Width, formattedDescription.Width) + 16 + padding * 2;

            double textAreaHeight = formattedTitle.Height + formattedDescription.Height + formattedCloseInfo.Height + padding * 5;

            double textAreaCenterX = textAreaWidth / 2 + highlightXOffset;
            double textAreaCenterY = textAreaHeight / 2 + highlightYOffset;

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
                    double windowWidth = AdornedElement.RenderSize.Width;
                    double windowHeight = AdornedElement.RenderSize.Height;

                    foreach (GuideTextPosition possiblePosition in Enum.GetValues(typeof(GuideTextPosition)))
                    {
                        if (possiblePosition == GuideTextPosition.Auto)
                        {
                            continue;
                        }

                        Rect possibleRect = GetTextAreaRect(possiblePosition, adornedElementRect, adornedElementCenter, 
                            formattedTitle, formattedDescription, formattedCloseInfo);

                        switch (possiblePosition)
                        {
                            case GuideTextPosition.Right:
                                if (possibleRect.Right <= windowWidth && possibleRect.Top >= 0 && possibleRect.Bottom <= windowHeight)
                                {
                                    return possibleRect;
                                }
                                break;
                            case GuideTextPosition.Bottom:
                                if (possibleRect.Right <= windowWidth && possibleRect.Left >= 0 && possibleRect.Bottom <= windowHeight)
                                {
                                    return possibleRect;
                                }
                                break;
                            case GuideTextPosition.Left:
                                if (possibleRect.Left >= 0 && possibleRect.Top >= 0 && possibleRect.Bottom <= windowHeight)
                                {
                                    return possibleRect;
                                }
                                break;
                            case GuideTextPosition.Top:
                                if (possibleRect.Right <= windowWidth && possibleRect.Left >= 0 && possibleRect.Top >= 0)
                                {
                                    return possibleRect;
                                }
                                break;
                        }
                    }
                    break;
            }

            //titlePosition.Offset(-padding, -padding);

            Rect textAreaRect = new Rect(titlePosition.X, titlePosition.Y, textAreaWidth, textAreaHeight);
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

            return textAreaRect;
        }

        private Point GetChildCenterPosition(Rect parent, Rect child, bool centerHorizontally,
            bool centerVertically)
        {
            double centerX = parent.X + parent.Width / 2 - child.Width / 2;
            double centerY = parent.Y + parent.Height / 2 - child.Height / 2;

            return new Point(centerHorizontally ? centerX : child.X,
                centerVertically ? centerY : child.Y);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            Closed?.Invoke(this, e);
        }
    }
}
