using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Enum
{
    public enum CabinSlotIssueType
    {
        STAIRWAY,
        DUPLICATE_SEAT,
        DOORS_DUPLICATE,
        DOORS_SERVICE_WRONG_SIDE,
        INVALID_POSITION_INTERIOR,
        INVALID_POSITION_COCKPIT,
        INVALID_POSITION_DOOR,
        SLOT_UNREACHABLE
    }
}
