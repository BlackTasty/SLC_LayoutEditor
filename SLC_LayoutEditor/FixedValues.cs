using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SLC_LayoutEditor
{
    class FixedValues
    {
        internal const string LOCAL_FILENAME = "Runtime.zip";

        internal const string AUTOFIX_STAIRWAY_KEY = "fix_stairway";

        internal static FontFamily FONT_FAMILY = new FontFamily("Segoe UI");
        internal const string URI_THEMES = "pack://application:,,,/SLC_LayoutEditor;component/Themes/Seasonal/";

        internal static readonly Brush DEFAULT_BRUSH = (Brush)App.Current.FindResource("ForegroundColorBrush");
        internal static readonly Brush DEFAULT_SECONDARY_BRUSH = (Brush)App.Current.FindResource("ForegroundSecondaryColorBrush");
        internal static readonly Brush GREEN_BRUSH = (Brush)App.Current.FindResource("ButtonBorderColorBrush");
        internal static readonly Brush YELLOW_BRUSH = (Brush)App.Current.FindResource("WarnForegroundColorBrush");
        internal static readonly Brush RED_BRUSH = (Brush)App.Current.FindResource("ErrorForegroundColorBrush");

        internal static readonly Brush PATCH_ADDED_BRUSH = (Brush)App.Current.FindResource("PatchAddedBrush");
        internal static readonly Brush PATCH_CHANGED_BRUSH = (Brush)App.Current.FindResource("PatchChangedBrush");
        internal static readonly Brush PATCH_FIXED_BRUSH = (Brush)App.Current.FindResource("PatchFixedBrush");
        internal static readonly Brush PATCH_REMOVED_BRUSH = (Brush)App.Current.FindResource("PatchRemovedBrush");
        internal static readonly Brush PATCH_DISABLED_BRUSH = (Brush)App.Current.FindResource("PatchDisabledBrush");

        internal static readonly Brush LIVE_GUIDE_OVERLAY_BRUSH = (Brush)App.Current.FindResource("LiveGuideOverlayBrush");
        internal static readonly Brush LIVE_GUIDE_TEXT_BORDER_BRUSH = (Brush)App.Current.FindResource("ButtonBorderColorBrush");
        internal static readonly Brush LIVE_GUIDE_TEXT_BACK_BRUSH = (Brush)App.Current.FindResource("BackgroundLightBrush");

        internal static readonly string GUIDE_CREATE_LAYOUT_TITLE = (string)App.Current.FindResource("GuideCreateLayoutTitle");
        internal static readonly string GUIDE_CREATE_LAYOUT_DESC = (string)App.Current.FindResource("GuideCreateLayoutDescription");
        internal static readonly string GUIDE_CREATE_TEMPLATE_TITLE = (string)App.Current.FindResource("GuideCreateTemplateTitle");
        internal static readonly string GUIDE_CREATE_TEMPLATE_DESC = (string)App.Current.FindResource("GuideCreateTemplateDescription");

        internal static readonly string GUIDE_SLOT_CONFIGURATOR_TITLE = (string)App.Current.FindResource("GuideSlotConfigurationTitle");
        internal static readonly string GUIDE_SLOT_CONFIGURATOR_DESC = (string)App.Current.FindResource("GuideSlotConfigurationDescription");
        internal static readonly string GUIDE_SLOT_AUTOMATION_TITLE = (string)App.Current.FindResource("GuideSlotAutomationTitle");
        internal static readonly string GUIDE_SLOT_AUTOMATION_DESC = (string)App.Current.FindResource("GuideSlotAutomationDescription");

        internal static readonly StreamGeometry ICON_CHEVRON_LEFT = (StreamGeometry)App.Current.FindResource("ChevronLeft");
        internal static readonly StreamGeometry ICON_CHEVRON_DOWN = (StreamGeometry)App.Current.FindResource("ChevronDown");
        internal static readonly StreamGeometry ICON_CHEVRON_RIGHT = (StreamGeometry)App.Current.FindResource("ChevronRight");

        internal static readonly Style STYLE_ICON_BUTTON = (Style)App.Current.FindResource("ForegroundIconButtonStyle");

        internal const string KEY_ISSUE_STAIRWAY = "stairway";
        internal const string KEY_ISSUE_ECO_CLASS = "eco_class";
        internal const string KEY_ISSUE_BUSINESS_CLASS = "business_class";
        internal const string KEY_ISSUE_PREMIUM_CLASS = "premium_class";
        internal const string KEY_ISSUE_FIRST_CLASS = "first_class";
        internal const string KEY_ISSUE_SUPERSONIC_CLASS = "supersonic_class";
        internal const string KEY_ISSUE_UNAVAILABLE_SEAT = "unavailable_seat";
        internal const string KEY_ISSUE_DOORS_DUPLICATE = "doors_duplicate";
        internal const string KEY_ISSUE_DOORS_SERVICE_WRONG_SIDE = "cat/lb";
        internal const string KEY_ISSUE_INVALID_INTERIOR_POSITIONS = "interior";
        internal const string KEY_ISSUE_UNREACHABLE_SLOTS = "unreachable";
    }
}
