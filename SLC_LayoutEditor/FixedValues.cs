using System.Windows.Media;

namespace SLC_LayoutEditor
{
    class FixedValues
    {
        internal const string LOCAL_FILENAME = "Runtime.zip";

        internal const string AUTOFIX_STAIRWAY_KEY = "fix_stairway";

        internal static readonly SolidColorBrush DEFAULT_BRUSH = (SolidColorBrush)App.Current.FindResource("ForegroundColorBrush");

        internal static readonly SolidColorBrush PATCH_ADDED_BRUSH = (SolidColorBrush)App.Current.FindResource("PatchAddedBrush");
        internal static readonly SolidColorBrush PATCH_CHANGED_BRUSH = (SolidColorBrush)App.Current.FindResource("PatchChangedBrush");
        internal static readonly SolidColorBrush PATCH_FIXED_BRUSH = (SolidColorBrush)App.Current.FindResource("PatchFixedBrush");
        internal static readonly SolidColorBrush PATCH_REMOVED_BRUSH = (SolidColorBrush)App.Current.FindResource("PatchRemovedBrush");
        internal static readonly SolidColorBrush PATCH_DISABLED_BRUSH = (SolidColorBrush)App.Current.FindResource("PatchDisabledBrush");

    }
}
