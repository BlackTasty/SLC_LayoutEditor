using SLC_LayoutEditor.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLC_LayoutEditor.Core.Guide
{
    class LiveGuideData
    {
        private readonly UIElement guidedElement;

        public UIElement GuidedElement => guidedElement;

        public LiveGuideData(UIElement guidedElement, GuideAssistOverrides overrides)
        {
            if (overrides?.AreOverridesSet ?? false)
            {
                GuideAssist.SetOverrides(guidedElement, overrides);
            }
            this.guidedElement = guidedElement;
        }
    }
}
