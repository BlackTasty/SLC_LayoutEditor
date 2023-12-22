﻿using System;
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
        GuideAdornerShown,
        GuideAdornerClosed,
        ForceTemplatingToggleState
    }
}
