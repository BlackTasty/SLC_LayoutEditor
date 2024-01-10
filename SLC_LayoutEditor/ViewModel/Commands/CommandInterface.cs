using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.Commands
{
    internal static class CommandInterface
    {
        private static readonly MakeTemplateCommand _makeTemplateCommand = new MakeTemplateCommand();
        private static readonly OpenLayoutFolderCommand _openLayoutFolderCommand = new OpenLayoutFolderCommand();
        private static readonly OpenLayoutInTextEditor _openLayoutInTextEditor = new OpenLayoutInTextEditor();
        private static readonly RedoCommand _redoCommand = new RedoCommand();
        private static readonly ReloadLayoutCommand _reloadLayoutCommand = new ReloadLayoutCommand();
        private static readonly RenameLayoutCommand _renameLayoutCommand = new RenameLayoutCommand();
        private static readonly SaveLayoutCommand _saveLayoutCommand = new SaveLayoutCommand();
        private static readonly SaveLayoutAsCommand _saveLayoutAsCommand = new SaveLayoutAsCommand();
        private static readonly SelectAllSlotsCommand _selectAllSlotsCommand = new SelectAllSlotsCommand();
        private static readonly SelectAllSlotsOnDeckCommand _selectAllSlotsOnDeckCommand = new SelectAllSlotsOnDeckCommand();
        private static readonly ShowKeybindsWindowCommand _showKeybindsWindowCommand = new ShowKeybindsWindowCommand();
        private static readonly UndoCommand _undoCommand = new UndoCommand();

        private static readonly CancelDialogCommand _cancelDialogCommand = new CancelDialogCommand();

        public static MakeTemplateCommand MakeTemplateCommand => _makeTemplateCommand;

        public static OpenLayoutFolderCommand OpenLayoutFolderCommand => _openLayoutFolderCommand;

        public static OpenLayoutInTextEditor OpenLayoutInTextEditor => _openLayoutInTextEditor;

        public static RedoCommand RedoCommand => _redoCommand;

        public static ReloadLayoutCommand ReloadLayoutCommand => _reloadLayoutCommand;

        public static RenameLayoutCommand RenameLayoutCommand => _renameLayoutCommand;

        public static SaveLayoutCommand SaveLayoutCommand => _saveLayoutCommand;

        public static SaveLayoutAsCommand SaveLayoutAsCommand => _saveLayoutAsCommand;

        public static SelectAllSlotsCommand SelectAllSlotsCommand => _selectAllSlotsCommand;

        public static SelectAllSlotsOnDeckCommand SelectAllSlotsOnDeckCommand => _selectAllSlotsOnDeckCommand;

        public static ShowKeybindsWindowCommand ShowKeybindsWindowCommand => _showKeybindsWindowCommand;

        public static UndoCommand UndoCommand => _undoCommand;

        public static CancelDialogCommand CancelDialogCommand => _cancelDialogCommand;
    }
}
