using SLC_LayoutEditor.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Enum
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AutomationMode
    {
        [Description("None")]
        None = -1,
        [Description("Seat numeration")]
        SeatNumeration,
        [Description("Wall generator")]
        WallGenerator,
        [Description("Service points")]
        ServicePoints
    }
}
