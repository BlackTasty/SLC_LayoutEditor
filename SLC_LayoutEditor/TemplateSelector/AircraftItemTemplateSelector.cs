using SLC_LayoutEditor.Core.Cabin;
using System.Windows;
using System.Windows.Controls;

namespace SLC_LayoutEditor.TemplateSelector
{
    class AircraftItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AircraftItemTemplate { get; set; }

        public DataTemplate TemplateItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CabinLayout cabinLayout)
            {
                if (cabinLayout.IsTemplate)
                {
                    return TemplateItemTemplate;
                }
                else
                {
                    return AircraftItemTemplate;
                }
            }

            return null;
        }
    }
}
