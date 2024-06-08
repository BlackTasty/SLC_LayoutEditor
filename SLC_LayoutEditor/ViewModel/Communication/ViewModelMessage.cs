using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.Communication
{
    public enum ViewModelMessage
    {
        DialogOpening,
        UnsavedChangesDialogClosed,
        SettingsSaved,
        Patcher_IsSearchingChanged,
        EditLayoutNameRequested,
        LayoutNameChanged,
        GuideAdornerShowing,
        GuideAdornerClosed,
        ForceTemplatingToggleState,
        Keybind_SelectAllSlotsOnDeck,
        Keybind_SaveLayout,
        Keybind_SaveLayoutAs,
        Keybind_MakeTemplate,
        Keybind_ReloadLayout,
        Keybind_Begin_CreateLayoutOrTemplate,
        Keybind_Begin_CreateAircraft,
        RefocusKeybindSheet,
        Notification_AddNotification,
        CreateSnapshot,
        FinishLayoutChange,
        Layout_ToggleIssueChecking,
        BakedTemplate_Add,
        BakedTemplate_Delete,
        HistoryStepApplied,
        SelectAll_Layout,
        SelectAll_Deck,
        DeselectOther_Deck,
        Layout_Load,
        Layout_Tile_Init,
        BackToLayoutOverview,
        Window_StateChanged,
        Layout_Tile_RefreshData,
        About_Show,
        Changelog_Show,
        Layout_Loading
    }
}
