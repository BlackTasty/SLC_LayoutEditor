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
        private readonly CabinDeckControl newValue;

        public CabinDeckControl NewValue => newValue;

        public SelectedDeckChangedEventArgs(CabinDeckControl newValue)
        {
            this.newValue = newValue;
        }
    }
}
