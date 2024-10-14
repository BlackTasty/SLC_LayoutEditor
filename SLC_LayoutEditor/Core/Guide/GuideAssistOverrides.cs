using SLC_LayoutEditor.Core.Enum;

namespace SLC_LayoutEditor.Core.Guide
{
    class GuideAssistOverrides
    {
        public double? Margin { get; set; }

        public double? Padding { get; set; }

        public double? CornerRadius { get; set; }

        public double? HighlightCornerRadius { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public GuideTextPosition? TextPosition { get; set; }

        public bool? HasGuide { get; set; }

        public bool? IsCircularCutout { get; set; }

        public double? WidthOffset { get; set; }

        public double? HeightOffset { get; set; }

        public double? RadiusOffset { get; set; }

        public double? HighlightXOffset { get; set; }

        public double? HighlightYOffset { get; set; }

        public double? SafeZoneSize { get; set; }

        public double? TextAreaXOffset { get; set; }

        public double? TextAreaYOffset { get; set; }

        public int? CurrentTourStep { get; set; }

        public int? TotalTourSteps { get; set; }

        public bool ApplyOverlayToAll { get; set; }

        public string TourStepCategory { get; set; }

        public bool AreTourStepsSet => CurrentTourStep != null && TotalTourSteps != null;

        public string GIFName { get; set; }

        public double GIFScaling { get; set; } = .75;

        public bool AreOverridesSet => Margin != null || Padding != null || CornerRadius != null || HighlightCornerRadius != null || Title != null || Description != null ||
            TextPosition != null || HasGuide != null || IsCircularCutout != null || WidthOffset != null || HeightOffset != null || RadiusOffset != null ||
            HighlightXOffset != null || HighlightYOffset != null || SafeZoneSize != null || TextAreaXOffset != null || TextAreaYOffset != null ||
            AreTourStepsSet;
    }
}
