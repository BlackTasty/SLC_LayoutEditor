using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Enum
{
    internal enum GuidedTourStep
    {
        Unset,
        Welcome,
        LayoutSelectionCard,
        EditorModeToggle,
        AircraftSelectionArea,
        LayoutsOverview,
        AircraftActionsArea,
        CreateLayoutFromOverview,
        CreateTemplate,
        DeleteAircraft,
        CreateLayout,
        BasicLayoutActionsArea,
        MakeTemplate,
        ReloadLayout,
        DeleteLayout,
        AddDeck,
        EditLayoutName,
        SidebarToggle,
        FirstLayoutIntroduction,
        SlotConfiguratorArea,
        IssueTracker,
        LayoutIssueCards,
        DeckIssueCards,
        SelectingSlots,
        DeselectingSlots,
        RowAndColumnSelect,
        PlacingEssentials,
        CompletingTheInterior,
        SlotConfiguratorToggle,
        SelectingSeatAutomation,
        SeatAutomationSettings,
        ServicePoints,
        ServicePointAutomationSettings,
        AutoFixingIssues,
        SaveLayout,
        SaveLayoutAs,
        FinalWords
    }
}
