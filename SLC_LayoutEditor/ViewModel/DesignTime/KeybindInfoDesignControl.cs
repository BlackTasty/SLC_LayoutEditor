using SLC_LayoutEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.ViewModel.DesignTime
{
    internal class KeybindInfoDesignControl : KeybindInfo
    {
        public new string Keybind => "Shift";
        public new string Title => "Test title";
        public new string Notes => "This is a test note, do be afraid";
    }
}
