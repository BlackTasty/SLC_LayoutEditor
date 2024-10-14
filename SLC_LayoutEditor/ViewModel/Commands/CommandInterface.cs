using SLC_LayoutEditor.ViewModel.Commands.SlotType;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal static class CommandInterface
    {
        private static readonly ToggleLogOutputCommand _toggleLogOutputCommand = new ToggleLogOutputCommand();

        private static readonly CreateCabinLayoutCommand _createCabinLayoutCommand = new CreateCabinLayoutCommand();
        private static readonly CreateTemplateCommand _createTemplateCommand = new CreateTemplateCommand();
        private static readonly CreateAircraftCommand _createAircraftCommand = new CreateAircraftCommand();
        private static readonly MakeTemplateCommand _makeTemplateCommand = new MakeTemplateCommand();
        private static readonly OpenLayoutFolderCommand _openLayoutFolderCommand = new OpenLayoutFolderCommand();
        private static readonly OpenLayoutInTextEditor _openLayoutInTextEditor = new OpenLayoutInTextEditor();
        private static readonly ReloadLayoutCommand _reloadLayoutCommand = new ReloadLayoutCommand();
        private static readonly RenameLayoutCommand _renameLayoutCommand = new RenameLayoutCommand();
        private static readonly SaveLayoutCommand _saveLayoutCommand = new SaveLayoutCommand();
        private static readonly SaveLayoutAsCommand _saveLayoutAsCommand = new SaveLayoutAsCommand();
        private static readonly SelectAllSlotsCommand _selectAllSlotsCommand = new SelectAllSlotsCommand();
        private static readonly SelectAllSlotsOnDeckCommand _selectAllSlotsOnDeckCommand = new SelectAllSlotsOnDeckCommand();
        private static readonly ShowKeybindsWindowCommand _showKeybindsWindowCommand = new ShowKeybindsWindowCommand();
        private static readonly ShowAboutDialogCommand _showAboutDialogCommand = new ShowAboutDialogCommand();

        private static readonly UndoCommand _undoCommand = new UndoCommand();
        private static readonly RedoCommand _redoCommand = new RedoCommand();
        private static readonly UndoUntilCommand _undoUntilCommand = new UndoUntilCommand();
        private static readonly RedoUntilCommand _redoUntilCommand = new RedoUntilCommand();

        private static readonly CancelDialogCommand _cancelDialogCommand = new CancelDialogCommand();

        #region Cabin slot type commands
        private static readonly SlotTypeAisleCommand _slotTypeAisleCommand = new SlotTypeAisleCommand();
        private static readonly SlotTypeBusinessClassCommand _slotTypeBusinessClassCommand = new SlotTypeBusinessClassCommand();
        private static readonly SlotTypeCateringDoorCommand _slotTypeCateringDoorCommand = new SlotTypeCateringDoorCommand();
        private static readonly SlotTypeDoorCommand _slotTypeDoorCommand = new SlotTypeDoorCommand();
        private static readonly SlotTypeCockpitCommand _slotTypeCockpitCommand = new SlotTypeCockpitCommand();
        private static readonly SlotTypeEconomyClassCommand _slotTypeEconomyClassCommand = new SlotTypeEconomyClassCommand();
        private static readonly SlotTypeFirstClassCommand _slotTypeFirstClassCommand = new SlotTypeFirstClassCommand();
        private static readonly SlotTypeGalleyCommand _slotTypeGalleyCommand = new SlotTypeGalleyCommand();
        private static readonly SlotTypeIntercomCommand _slotTypeIntercomCommand = new SlotTypeIntercomCommand();
        private static readonly SlotTypeKitchenCommand _slotTypeKitchenCommand = new SlotTypeKitchenCommand();
        private static readonly SlotTypeLoadingBayCommand _slotTypeLoadingBayCommand = new SlotTypeLoadingBayCommand();
        private static readonly SlotTypePremiumClassCommand _slotTypePremiumClassCommand = new SlotTypePremiumClassCommand();
        private static readonly SlotTypeServiceEndCommand _slotTypeServiceEndCommand = new SlotTypeServiceEndCommand();
        private static readonly SlotTypeServiceStartCommand _slotTypeServiceStartCommand = new SlotTypeServiceStartCommand();
        private static readonly SlotTypeStairwayCommand _slotTypeStairwayCommand = new SlotTypeStairwayCommand();
        private static readonly SlotTypeSupersonicClassCommand _slotTypeSupersonicClassCommand = new SlotTypeSupersonicClassCommand();
        private static readonly SlotTypeToiletCommand _slotTypeToiletCommand = new SlotTypeToiletCommand();
        private static readonly SlotTypeUnavailableSeatCommand _slotTypeUnavailableSeatCommand = new SlotTypeUnavailableSeatCommand();
        private static readonly SlotTypeWallCommand _slotTypeWallCommand = new SlotTypeWallCommand();
        #endregion

        #region Debug commands
        public static ToggleLogOutputCommand ToggleLogOutputCommand => _toggleLogOutputCommand;
        #endregion

        public static CreateCabinLayoutCommand CreateCabinLayoutCommand => _createCabinLayoutCommand;

        public static CreateTemplateCommand CreateTemplateCommand => _createTemplateCommand;

        public static CreateAircraftCommand CreateAircraftCommand => _createAircraftCommand;

        public static MakeTemplateCommand MakeTemplateCommand => _makeTemplateCommand;

        public static OpenLayoutFolderCommand OpenLayoutFolderCommand => _openLayoutFolderCommand;

        public static OpenLayoutInTextEditor OpenLayoutInTextEditor => _openLayoutInTextEditor;

        public static ReloadLayoutCommand ReloadLayoutCommand => _reloadLayoutCommand;

        public static RenameLayoutCommand RenameLayoutCommand => _renameLayoutCommand;

        public static SaveLayoutCommand SaveLayoutCommand => _saveLayoutCommand;

        public static SaveLayoutAsCommand SaveLayoutAsCommand => _saveLayoutAsCommand;

        public static SelectAllSlotsCommand SelectAllSlotsCommand => _selectAllSlotsCommand;

        public static SelectAllSlotsOnDeckCommand SelectAllSlotsOnDeckCommand => _selectAllSlotsOnDeckCommand;

        public static ShowKeybindsWindowCommand ShowKeybindsWindowCommand => _showKeybindsWindowCommand;

        public static ShowAboutDialogCommand ShowAboutDialogCommand => _showAboutDialogCommand;

        public static UndoCommand UndoCommand => _undoCommand;

        public static RedoCommand RedoCommand => _redoCommand;

        public static UndoUntilCommand UndoUntilCommand => _undoUntilCommand;

        public static RedoUntilCommand RedoUntilCommand => _redoUntilCommand;

        public static CancelDialogCommand CancelDialogCommand => _cancelDialogCommand;

        #region Cabin slot type commands
        public static SlotTypeAisleCommand SlotTypeAisleCommand => _slotTypeAisleCommand;

        public static SlotTypeBusinessClassCommand SlotTypeBusinessClassCommand => _slotTypeBusinessClassCommand;

        public static SlotTypeCateringDoorCommand SlotTypeCateringDoorCommand => _slotTypeCateringDoorCommand;

        public static SlotTypeDoorCommand SlotTypeDoorCommand => _slotTypeDoorCommand;

        public static SlotTypeCockpitCommand SlotTypeCockpitCommand => _slotTypeCockpitCommand;

        public static SlotTypeEconomyClassCommand SlotTypeEconomyClassCommand => _slotTypeEconomyClassCommand;

        public static SlotTypeFirstClassCommand SlotTypeFirstClassCommand => _slotTypeFirstClassCommand;

        public static SlotTypeGalleyCommand SlotTypeGalleyCommand => _slotTypeGalleyCommand;

        public static SlotTypeIntercomCommand SlotTypeIntercomCommand => _slotTypeIntercomCommand;

        public static SlotTypeKitchenCommand SlotTypeKitchenCommand => _slotTypeKitchenCommand;

        public static SlotTypeLoadingBayCommand SlotTypeLoadingBayCommand => _slotTypeLoadingBayCommand;

        public static SlotTypePremiumClassCommand SlotTypePremiumClassCommand => _slotTypePremiumClassCommand;

        public static SlotTypeServiceEndCommand SlotTypeServiceEndCommand => _slotTypeServiceEndCommand;

        public static SlotTypeServiceStartCommand SlotTypeServiceStartCommand => _slotTypeServiceStartCommand;

        public static SlotTypeStairwayCommand SlotTypeStairwayCommand => _slotTypeStairwayCommand;

        public static SlotTypeSupersonicClassCommand SlotTypeSupersonicClassCommand => _slotTypeSupersonicClassCommand;

        public static SlotTypeToiletCommand SlotTypeToiletCommand => _slotTypeToiletCommand;

        public static SlotTypeUnavailableSeatCommand SlotTypeUnavailableSeatCommand => _slotTypeUnavailableSeatCommand;

        public static SlotTypeWallCommand SlotTypeWallCommand => _slotTypeWallCommand;
        #endregion
    }
}
