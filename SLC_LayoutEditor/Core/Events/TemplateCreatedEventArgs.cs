using SLC_LayoutEditor.Core.Cabin;
using System;

namespace SLC_LayoutEditor.Core.Events
{
    public class TemplateCreatedEventArgs : EventArgs
    {
        private readonly CabinLayout template;

        public CabinLayout Template => template;

        public TemplateCreatedEventArgs(CabinLayout template)
        {
            this.template = template;
        }
    }
}
