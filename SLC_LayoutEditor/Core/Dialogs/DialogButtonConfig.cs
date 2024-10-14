using SLC_LayoutEditor.Core.Enum;

namespace SLC_LayoutEditor.Core.Dialogs
{
    public class DialogButtonConfig
    {
        private readonly string text;
        private readonly DialogButtonStyle style;
        private readonly bool isCancel;

        public string Text => text;

        public DialogButtonStyle Style => style;

        public bool IsCancel => isCancel;

        public DialogButtonConfig(string text) : this(text, DialogButtonStyle.Green)
        {

        }

        public DialogButtonConfig(string text, DialogButtonStyle style) 
            : this(text, style, false)
        {

        }

        public DialogButtonConfig(string text, DialogButtonStyle style, bool isCancel)
        {
            this.text = text;
            this.style = style;
            this.isCancel = isCancel;
        }
    }
}
