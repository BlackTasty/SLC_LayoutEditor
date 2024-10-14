namespace SLC_LayoutEditor.Core.Cabin
{
    internal class CabinSlotIssue
    {
        private readonly bool isProblematic;
        private bool hideHighlighting;

        public bool IsProblematic => !hideHighlighting ? isProblematic : false;

        public bool HideHighlighting
        {
            set
            {
                hideHighlighting = value;
            }
        }

        public CabinSlotIssue(bool hideHighlighting = false)
        {
            isProblematic = true;
            this.hideHighlighting = hideHighlighting;
        }
    }
}
