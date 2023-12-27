using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Enum;
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

        private readonly Window window;
        private readonly UIElement guidedElement;

        public UIElement GuidedElement => guidedElement;

        public static Adorner AttachAdorner(UIElement rootElement, UIElement guidedElement)
        {
            Adorner adorner = rootElement.AttachAdorner(typeof(LiveGuideAdorner), (UIElement)guidedElement, GuideAssist.GetMargin(guidedElement), GuideAssist.GetPadding(guidedElement),
                GuideAssist.GetCornerRadius(guidedElement), GuideAssist.GetTitle(guidedElement), GuideAssist.GetDescription(guidedElement), 
                FixedValues.LIVE_GUIDE_OVERLAY_BRUSH, Window.GetWindow(guidedElement), GuideAssist.GetIsCircleCutout(guidedElement), 
                GuideAssist.GetTextPosition(guidedElement), GuideAssist.GetTextAreaXOffset(guidedElement), GuideAssist.GetTextAreaYOffset(guidedElement), 
                GuideAssist.GetWidthOffset(guidedElement), GuideAssist.GetHeightOffset(guidedElement),
                GuideAssist.GetRadiusOffset(guidedElement), GuideAssist.GetHighlightXOffset(guidedElement), GuideAssist.GetHighlightYOffset(guidedElement),
                GuideAssist.GetSafeZoneSize(guidedElement));

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
            double highlightXOffset, double highlightYOffset, double safeZoneSize) : base(rootElement)
        {
            this.guidedElement = guidedElement;

            this.radiusOffset = radiusOffset;
            this.widthOffset = widthOffset;
            this.heightOffset = heightOffset;
            this.highlightXOffset = highlightXOffset;
            this.highlightYOffset = highlightYOffset;
            this.textAreaXOffset = textAreaXOffset;
            this.textAreaYOffset = textAreaYOffset;
            this.safeZoneSize = safeZoneSize;

            this.isCircularCutout = isCircularCutout;
            this.margin = margin;
            this.padding = padding;
            this.cornerRadius = cornerRadius;
            this.radius = GetRadius(guidedElement, margin);
            this.title = title;
            this.description = description;
            this.textPosition = textPosition;
            this.overlayBrush = overlayBrush;

            this.window = window;
            PreviewMouseUp += LiveGuideAdorner_PreviewMouseUp;
            ClipToBounds = false;
        }

        private void LiveGuideAdorner_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AdornedElement.RemoveAdorner(this);
            Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerClosed);
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
            FormattedText formattedCloseInfo = GetFormattedText("Click anywhere to close this info", FontWeights.Thin, 10, FixedValues.YELLOW_BRUSH);

            Rect textAreaRect = GetTextAreaRect(textPosition, adornedElementRect, adornedElementCenter, formattedTitle, formattedDescription, formattedCloseInfo);

            Point titlePosition = textAreaRect.Location.MakeOffset(padding, 0);
            Point descriptionPosition = titlePosition.MakeOffset(8, formattedTitle.Height + 8);
            Point closeInfoPosition = descriptionPosition.MakeOffset(0, formattedDescription.Height + 12);

            ApplyOpacityMask(drawingContext, drawingRect, adornedElementRect, adornedElementCenter);
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

        private void ApplyOpacityMask(DrawingContext context, Rect drawingRect, Rect adornedElementRect, Point adornedElementCenter)
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
                geometry = new CombinedGeometry(GeometryCombineMode.Xor,
                   new RectangleGeometry(drawingRect),
                   new RectangleGeometry(adjustedHighlightRect, cornerRadius, cornerRadius));
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
            double textAreaCenterX = Math.Max(formattedDescription.Width, formattedTitle.Width) / 2 + highlightXOffset;
            double textAreaCenterY = Math.Max(formattedDescription.Height, formattedTitle.Height) / 2 + highlightYOffset;

            double textAreaWidth = Math.Max(formattedTitle.Width, formattedDescription.Width + 8) + padding * 2;
            double textAreaHeight = formattedTitle.Height + formattedDescription.Height + formattedCloseInfo.Height + padding * 4;

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
            double totalRightOffset = textAreaRect.Right + safeZoneSize;
            totalRightOffset = (totalRightOffset > AdornedElement.RenderSize.Width ? AdornedElement.RenderSize.Width - totalRightOffset : 0) + textAreaXOffset;
            if (totalRightOffset < 0)
            {
                textAreaRect.Offset(totalRightOffset, 0);
            }

            double totalTopOffset = textAreaRect.Top - safeZoneSize;
            totalTopOffset = (totalTopOffset > AdornedElement.RenderSize.Height ? AdornedElement.RenderSize.Height - totalTopOffset : 0) + textAreaYOffset;
            if (totalTopOffset < 0)
            {
                textAreaRect.Offset(0, totalTopOffset);
            }

            double totalLeftOffset = textAreaRect.Left - safeZoneSize;
            totalLeftOffset = (totalLeftOffset < 0 ? Math.Abs(totalLeftOffset) : 0) + textAreaXOffset;
            if (totalLeftOffset > 0)
            {
                textAreaRect.Offset(totalLeftOffset, 0);
            }

            double totalBottomOffset = textAreaRect.Bottom + safeZoneSize;
            totalBottomOffset = (totalBottomOffset < 0 ? Math.Abs(totalBottomOffset) : 0) + textAreaYOffset;
            if (totalBottomOffset > 0)
            {
                textAreaRect.Offset(0, totalBottomOffset);
            }

            return textAreaRect;
        }
    }
}
