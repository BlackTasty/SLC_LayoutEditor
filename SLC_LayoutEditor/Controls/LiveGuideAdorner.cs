using SLC_LayoutEditor.Core;
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
        private readonly double margin;
        private readonly double padding;
        private readonly double cornerRadius;

        private readonly double widthOffset;
        private readonly double heightOffset;
        private readonly double radiusOffset;
        private readonly double highlightXOffset;
        private readonly double highlightYOffset;

        private readonly double radius;
        private readonly string title;
        private readonly string description;
        private readonly Brush overlayBrush;
        private readonly Dock textPosition;
        private readonly bool isCircularCutout;

        private readonly Window window;

        public static Adorner AttachAdorner(UIElement uIElement)
        {
            string title = GuideAssist.GetTitle(uIElement);
            string description = GuideAssist.GetDescription(uIElement);
            double margin = GuideAssist.GetMargin(uIElement);
            double padding = GuideAssist.GetPadding(uIElement);
            double cornerRadius = GuideAssist.GetCornerRadius(uIElement);
            Dock textPosition = GuideAssist.GetTextPosition(uIElement);
            bool isCircularCutout = GuideAssist.GetIsCircleCutout(uIElement);

            double widthOffset = GuideAssist.GetWidthOffset(uIElement);
            double heightOffset = GuideAssist.GetHeightOffset(uIElement);
            double radiusOffset = GuideAssist.GetRadiusOffset(uIElement);
            double highlightXOffset = GuideAssist.GetHighlightXOffset(uIElement);
            double highlightYOffset = GuideAssist.GetHighlightYOffset(uIElement);

            Adorner adorner = uIElement.AttachAdorner(typeof(LiveGuideAdorner), margin, padding, cornerRadius, title, description, textPosition,
                FixedValues.LIVE_GUIDE_OVERLAY_BRUSH, Window.GetWindow(uIElement), isCircularCutout, 
                widthOffset, heightOffset, radiusOffset, highlightXOffset, highlightYOffset);

            Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerShown, adorner);

            return adorner;
        }

        /// <summary>
        /// Attach the adorner to an element.
        /// </summary>
        /// <param name="adornedElement">The element to attach the adorner to</param>
        /// <param name="margin">The margin between the circle around the adorned element and the element itself</param>
        /// <param name="title">The title for this guide</param>
        /// <param name="description">A short description describing the action</param>
        /// <param name="overlayBrush">The color of the overlay blending out other elements</param>
        /// <param name="windowSize">The size of the window</param>
        public LiveGuideAdorner(UIElement adornedElement, double margin, double padding, double cornerRadius, string title, string description,
            Dock textPosition, Brush overlayBrush, Window window, bool isCircularCutout, 
            double widthOffset, double heightOffset, double radiusOffset, double highlightXOffset, double highlightYOffset) : base(adornedElement)
        {
            this.radiusOffset = radiusOffset;
            this.widthOffset = widthOffset;
            this.heightOffset = heightOffset;
            this.highlightXOffset = highlightXOffset;
            this.highlightYOffset = highlightYOffset;

            this.isCircularCutout = isCircularCutout;
            this.margin = margin;
            this.padding = padding;
            this.cornerRadius = cornerRadius;
            this.radius = GetRadius(adornedElement, margin);
            this.title = title;
            this.description = description;
            this.textPosition = textPosition;
            this.overlayBrush = overlayBrush;

            this.window = window;
            PreviewMouseUp += LiveGuideAdorner_PreviewMouseUp;
        }

        private void LiveGuideAdorner_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AdornedElement.RemoveAdorner(this);
            Mediator.Instance.NotifyColleagues(ViewModelMessage.GuideAdornerClosed);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(AdornedElement.DesiredSize);
            
            if (AdornedElement is FrameworkElement element)
            {
                if (adornedElementRect.Width < element.ActualWidth)
                {
                    adornedElementRect.Width = element.ActualWidth;
                }

                if (adornedElementRect.Height < element.ActualHeight)
                {
                    adornedElementRect.Height = element.ActualHeight;
                }
            }
            adornedElementRect.Width += widthOffset;
            adornedElementRect.Height += heightOffset;

            Point adornedElementCenter = new Point(adornedElementRect.Width / 2, adornedElementRect.Height / 2);
            adornedElementCenter.Offset(highlightXOffset, highlightYOffset);
            Point relativePosition = AdornedElement.TransformToAncestor(window).Transform(new Point(0, 0));

            // Generate texts
            FormattedText formattedTitle = GetFormattedText(title, FontWeights.Bold, 32, FixedValues.DEFAULT_BRUSH);
            FormattedText formattedDescription = GetFormattedText(description, FontWeights.Thin, 16, FixedValues.DEFAULT_SECONDARY_BRUSH);
            FormattedText formattedCloseInfo = GetFormattedText("Click anywhere to close this info", FontWeights.Thin, 10, FixedValues.YELLOW_BRUSH);
            Point titlePosition = GetTitlePosition(adornedElementRect, adornedElementCenter, formattedTitle, formattedDescription);

            double textAreaWidth = Math.Max(formattedTitle.Width, formattedDescription.Width + 8) + padding * 2;
            double textAreaHeight = formattedTitle.Height + formattedDescription.Height + formattedCloseInfo.Height + padding * 4;
            Rect textAreaRect = new Rect(titlePosition.X - padding, titlePosition.Y - padding, 
                textAreaWidth,
                textAreaHeight);

            double totalX = titlePosition.X + relativePosition.X + textAreaWidth + 32;
            totalX = totalX > window.ActualWidth ? window.ActualWidth - totalX : 0;
            double totalY = titlePosition.Y + relativePosition.Y + textAreaHeight + 32;
            totalY = totalY > window.ActualHeight ? window.ActualHeight - totalY : 0;

            titlePosition.Offset(totalX, totalY);
            textAreaRect.Offset(totalX, totalY);

            Point descriptionPosition = titlePosition.MakeOffset(4, formattedTitle.Height + 8);
            Point closeInfoPosition = descriptionPosition.MakeOffset(0, formattedDescription.Height + 12);

            Rect drawingRect = new Rect(-relativePosition.X, -relativePosition.Y, window.ActualWidth, window.ActualHeight);

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
                geometry = new CombinedGeometry(GeometryCombineMode.Xor,
                   new RectangleGeometry(drawingRect),
                   new EllipseGeometry(adornedElementCenter, radius, radius));
            }
            else
            {
                geometry = new CombinedGeometry(GeometryCombineMode.Xor,
                   new RectangleGeometry(drawingRect),
                   new RectangleGeometry(adornedElementRect, cornerRadius, cornerRadius));
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

        private Point GetTitlePosition(Rect adornedElementRect, Point adornedElementCenter, FormattedText formattedTitle, FormattedText formattedDescription)
        {
            double offset = (isCircularCutout ? radius + margin : margin * 3);

            switch (textPosition)
            {
                case Dock.Right:
                    return new Point(adornedElementRect.Width + offset, adornedElementRect.Top - offset + margin);
                case Dock.Bottom:
                    return new Point(adornedElementCenter.X, adornedElementRect.Height + offset);
                case Dock.Left:
                    return new Point(0 - formattedDescription.Width - offset - 16, adornedElementRect.Top - offset + margin);
                case Dock.Top:
                    return new Point(adornedElementCenter.X, 0 - offset - formattedTitle.Height - formattedDescription.Height);
                default:
                    return new Point();
            }
        }
    }
}
