using SLC_LayoutEditor.Core.Enum;
using System.Collections.Generic;
using System.Linq;
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

        internal static readonly string ICON_CLIPBOARD = (string)App.Current.FindResource("Clipboard");
        internal static readonly string ICON_FILE_OPENED = (string)App.Current.FindResource("OpenInNew");
        internal static readonly string ICON_BROWSER_OPENED = (string)App.Current.FindResource("OpenInBrowser");
        internal static readonly string ICON_CHECK_CIRCLE = (string)App.Current.FindResource("CheckCircle");
        internal static readonly string ICON_BACKUP_RESTORE = (string)App.Current.FindResource("BackupRestore");
        internal static readonly string ICON_SAVE = (string)App.Current.FindResource("SaveConfirm");

        internal static readonly string MAXIMIZE_ICON = (string)App.Current.FindResource("WindowMaximize");
        internal static readonly string RESTORE_ICON = (string)App.Current.FindResource("WindowRestore");

        internal readonly static int TOTAL_TOUR_STEPS = (int)System.Enum.GetValues(typeof(GuidedTourStep)).Cast<GuidedTourStep>().Max();

        internal const double DEFAULT_BORDER_THICKNESS = 1.5;

        internal const double DECK_BASE_MARGIN = 16;
        internal const double LAYOUT_OFFSET_X = 64;
        internal const double LAYOUT_OFFSET_Y = 64;
        internal static Size SLOT_DIMENSIONS = new Size(40, 40);
    }
}
