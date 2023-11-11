using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public class Change
    {
        private readonly IHistorical previousState;
        private readonly StateChangeType changeType;

        public IHistorical PreviousState => previousState;

        public StateChangeType ChangeType => changeType;

        public Change(IHistorical previousState, StateChangeType changeType)
        {
            this.previousState = previousState;
            this.changeType = changeType;
        }
    }
}
