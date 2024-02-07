using SLC_LayoutEditor.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinSlotIssues : ViewModelBase
    {
        public event EventHandler<EventArgs> ProblematicChanged;

        private Dictionary<CabinSlotIssueType, CabinSlotIssue> listedIssues = new Dictionary<CabinSlotIssueType, CabinSlotIssue>();

        private bool wasProblematic;
        private readonly CabinSlot parent;

        public bool IsProblematic => listedIssues.Any(x => x.Value.IsProblematic);

        public bool IsEvaluationActive { get; set; } = true;

        public CabinSlotIssues(CabinSlot parent)
        {
            this.parent = parent;
        }

        public void ToggleIssue(CabinSlotIssueType issue, bool isProblematic, bool hideHighlighting = false)
        {
            bool wasProblematic = IsProblematic;

            if (isProblematic && !listedIssues.ContainsKey(issue))
            {
                listedIssues.Add(issue, new CabinSlotIssue(hideHighlighting));
            }
            else if (!isProblematic && listedIssues.ContainsKey(issue))
            {
                listedIssues.Remove(issue);
            }

            RefreshProblematicFlag(wasProblematic != IsProblematic);
        }

        public void RefreshProblematicFlag(bool hasStateChanged)
        {
            if (hasStateChanged)
            {
                parent.IsDirty = true;
                OnProblematicChanged(EventArgs.Empty);
                InvokePropertyChanged(nameof(IsProblematic));
            }
        }

        public void ToggleIssueHighlighting(CabinSlotIssueType issue, bool showHighlighting)
        {
            if (listedIssues.ContainsKey(issue))
            {
                listedIssues[issue].HideHighlighting = !showHighlighting;
            }
            RefreshProblematicFlag(true);
        }

        public void ClearIssues()
        {
            bool wasProblematic = IsProblematic;
            listedIssues.Clear();
            RefreshProblematicFlag(wasProblematic != IsProblematic);
        }

        public bool HasIssue(CabinSlotIssueType issue)
        {
            return listedIssues.ContainsKey(issue);
        }

        protected virtual void OnProblematicChanged(EventArgs e)
        {
            if (IsEvaluationActive)
            {
                ProblematicChanged?.Invoke(this, e);
            }
        }
    }
}
