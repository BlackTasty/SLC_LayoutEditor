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
    public enum CabinSlotType
    {
        [Description("Aisle")]
        Aisle, // "  -  "
        [Description("Wall")]
        Wall, // "  X  "
        [Description("Door")]
        Door, // " D-X " e.g. " D-1 "
        [Description("Cockpit")]
        Cockpit, // "  C  "
        [Description("Galley")]
        Galley, // "  G  "
        [Description("Toilet")]
        Toilet, // "  T  "
        [Description("Stairway")]
        Stairway, // "  S  "
        [Description("Kitchen")]
        Kitchen, // "  K  "
        [Description("Intercom")]
        Intercom, // "  I  "
        [Description("Business Class")]
        BusinessClassSeat, // "B-XXX" - e.g. "B-12A"
        [Description("Economy Class")]
        EconomyClassSeat, // "E-XXX" - e.g. "E-05C"
        [Description("First Class")]
        FirstClassSeat, // "F-XXX"  e.g. "F-37A"
        [Description("Premium Economy Class")]
        PremiumClassSeat, // "P-XXX" - e.g. "P-01B"
        [Description("Supersonic Class")]
        SupersonicClassSeat, // "R-XXX" - e.g. "R-30C"
        [Description("Seat, but Unavailable")]
        UnavailableSeat, // "U-XXX - e.g. "U-03H"
        [Description("Service START POINT")]
        ServiceStartPoint, // "  <  "
        [Description("Service END POINT")]
        ServiceEndPoint, // "  >  "
    }
}
