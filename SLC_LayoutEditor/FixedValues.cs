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
        internal static readonly Brush YELLOW_BRUSH = (Brush)App.Current.FindResource("WarnForegroundColorBrush");

        internal static readonly Brush PATCH_ADDED_BRUSH = (Brush)App.Current.FindResource("PatchAddedBrush");
        internal static readonly Brush PATCH_CHANGED_BRUSH = (Brush)App.Current.FindResource("PatchChangedBrush");
        internal static readonly Brush PATCH_FIXED_BRUSH = (Brush)App.Current.FindResource("PatchFixedBrush");
        internal static readonly Brush PATCH_REMOVED_BRUSH = (Brush)App.Current.FindResource("PatchRemovedBrush");
        internal static readonly Brush PATCH_DISABLED_BRUSH = (Brush)App.Current.FindResource("PatchDisabledBrush");

        internal static readonly Brush LIVE_GUIDE_OVERLAY_BRUSH = (Brush)App.Current.FindResource("LiveGuideOverlayBrush");
        internal static readonly Brush LIVE_GUIDE_TEXT_BORDER_BRUSH = (Brush)App.Current.FindResource("ButtonBorderColorBrush");
        internal static readonly Brush LIVE_GUIDE_TEXT_BACK_BRUSH = (Brush)App.Current.FindResource("BackgroundLightBrush");
    }
}
