using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Guide;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.Controls
{
    internal class GuideAssist : UIElement
    {
        #region Margin property
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(16d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetMargin(UIElement target) =>
            (double)target.GetValue(MarginProperty);

        public static void SetMargin(UIElement target, double value) =>
            target.SetValue(MarginProperty, value);
        #endregion

        #region Padding property
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.RegisterAttached("Padding",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(8d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetPadding(UIElement target) =>
            (double)target.GetValue(PaddingProperty);

        public static void SetPadding(UIElement target, double value) =>
            target.SetValue(PaddingProperty, value);
        #endregion

        #region CornerRadius property
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(8d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetCornerRadius(UIElement target) =>
            (double)target.GetValue(CornerRadiusProperty);

        public static void SetCornerRadius(UIElement target, double value) =>
            target.SetValue(CornerRadiusProperty, value);
        #endregion

        #region Title property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title",
                typeof(string), typeof(GuideAssist), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

        public static string GetTitle(UIElement target) =>
            (string)target.GetValue(TitleProperty);

        public static void SetTitle(UIElement target, string value) =>
            target.SetValue(TitleProperty, value);
        #endregion

        #region Description property
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.RegisterAttached("Description",
                typeof(string), typeof(GuideAssist), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

        public static string GetDescription(UIElement target) =>
            (string)target.GetValue(DescriptionProperty);

        public static void SetDescription(UIElement target, string value) =>
            target.SetValue(DescriptionProperty, value);
        #endregion

        #region TextPosition property
        public static readonly DependencyProperty TextPositionProperty =
            DependencyProperty.RegisterAttached("TextPosition",
                typeof(GuideTextPosition), typeof(GuideAssist), new FrameworkPropertyMetadata(GuideTextPosition.Auto, FrameworkPropertyMetadataOptions.AffectsRender));

        public static GuideTextPosition GetTextPosition(UIElement target) =>
            (GuideTextPosition)target.GetValue(TextPositionProperty);

        public static void SetTextPosition(UIElement target, GuideTextPosition value) =>
            target.SetValue(TextPositionProperty, value);
        #endregion

        #region HasGuide property
        public static readonly DependencyProperty HasGuideProperty =
            DependencyProperty.RegisterAttached("HasGuide",
                typeof(bool), typeof(GuideAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetHasGuide(UIElement target) =>
            (bool)target.GetValue(HasGuideProperty);

        public static void SetHasGuide(UIElement target, bool value) =>
            target.SetValue(HasGuideProperty, value);
        #endregion

        #region IsCircularCutout property
        public static readonly DependencyProperty IsCircularCutoutProperty =
            DependencyProperty.RegisterAttached("IsCircularCutout",
                typeof(bool), typeof(GuideAssist), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetIsCircularCutout(UIElement target) =>
            (bool)target.GetValue(IsCircularCutoutProperty);

        public static void SetIsCircularCutout(UIElement target, bool value) =>
            target.SetValue(IsCircularCutoutProperty, value);
        #endregion

        #region WidthOffset property
        public static readonly DependencyProperty WidthOffsetProperty =
            DependencyProperty.RegisterAttached("WidthOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetWidthOffset(UIElement target) =>
            (double)target.GetValue(WidthOffsetProperty);

        public static void SetWidthOffset(UIElement target, double value) =>
            target.SetValue(WidthOffsetProperty, value);
        #endregion

        #region HeightOffset property
        public static readonly DependencyProperty HeightOffsetProperty =
            DependencyProperty.RegisterAttached("HeightOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetHeightOffset(UIElement target) =>
            (double)target.GetValue(HeightOffsetProperty);

        public static void SetHeightOffset(UIElement target, double value) =>
            target.SetValue(HeightOffsetProperty, value);
        #endregion

        #region RadiusOffset property
        public static readonly DependencyProperty RadiusOffsetProperty =
            DependencyProperty.RegisterAttached("RadiusOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetRadiusOffset(UIElement target) =>
            (double)target.GetValue(RadiusOffsetProperty);

        public static void SetRadiusOffset(UIElement target, double value) =>
            target.SetValue(RadiusOffsetProperty, value);
        #endregion

        #region HighlightXOffset property
        public static readonly DependencyProperty HighlightXOffsetProperty =
            DependencyProperty.RegisterAttached("HighlightXOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetHighlightXOffset(UIElement target) =>
            (double)target.GetValue(HighlightXOffsetProperty);

        public static void SetHighlightXOffset(UIElement target, double value) =>
            target.SetValue(HighlightXOffsetProperty, value);
        #endregion

        #region HighlightYOffset property
        public static readonly DependencyProperty HighlightYOffsetProperty =
            DependencyProperty.RegisterAttached("HighlightYOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetHighlightYOffset(UIElement target) =>
            (double)target.GetValue(HighlightYOffsetProperty);

        public static void SetHighlightYOffset(UIElement target, double value) =>
            target.SetValue(HighlightYOffsetProperty, value);
        #endregion

        #region SafeZoneSize property
        public static readonly DependencyProperty SafeZoneSizeProperty =
            DependencyProperty.RegisterAttached("SafeZoneSize",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(32d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetSafeZoneSize(UIElement target) =>
            (double)target.GetValue(SafeZoneSizeProperty);

        public static void SetSafeZoneSize(UIElement target, double value) =>
            target.SetValue(SafeZoneSizeProperty, value);
        #endregion

        #region TextAreaXOffset property
        public static readonly DependencyProperty TextAreaXOffsetProperty =
            DependencyProperty.RegisterAttached("TextAreaXOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetTextAreaXOffset(UIElement target) =>
            (double)target.GetValue(TextAreaXOffsetProperty);

        public static void SetTextAreaXOffset(UIElement target, double value) =>
            target.SetValue(TextAreaXOffsetProperty, value);
        #endregion

        #region TextAreaYOffset property
        public static readonly DependencyProperty TextAreaYOffsetProperty =
            DependencyProperty.RegisterAttached("TextAreaYOffset",
                typeof(double), typeof(GuideAssist), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

        public static double GetTextAreaYOffset(UIElement target) =>
            (double)target.GetValue(TextAreaYOffsetProperty);

        public static void SetTextAreaYOffset(UIElement target, double value) =>
            target.SetValue(TextAreaYOffsetProperty, value);
        #endregion

        #region Overrides
        public static readonly DependencyProperty OverridesProperty =
            DependencyProperty.RegisterAttached("Overrides",
                typeof(GuideAssistOverrides), typeof(GuideAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static GuideAssistOverrides GetOverrides(UIElement target) =>
            (GuideAssistOverrides)target.GetValue(OverridesProperty);

        public static void SetOverrides(UIElement target, GuideAssistOverrides value) =>
            target.SetValue(OverridesProperty, value);
        #endregion
    }
}
