using SLC_LayoutEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Events
{
    public class SelectedDeckChangedEventArgs
    {
        private readonly DeckLayoutControl newValue;

        public DeckLayoutControl NewValue => newValue;

        public SelectedDeckChangedEventArgs(DeckLayoutControl newValue)
        {
            this.newValue = newValue;
        }
    }
}
