using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    public class CabinSlotIssues : ViewModelBase
    {
        public event EventHandler<EventArgs> ProblematicChanged;

        private Dictionary<string, CabinSlotIssue> listedIssues = new Dictionary<string, CabinSlotIssue>();

        public bool IsProblematic => listedIssues.Any(x => x.Value.IsProblematic);

        public bool IsEvaluationActive { get; set; } = true;

        public void ToggleIssue(string key, bool isProblematic)
        {
            if (isProblematic && !listedIssues.ContainsKey(key))
            {
                listedIssues.Add(key, new CabinSlotIssue());
            }
            else if (!isProblematic && listedIssues.ContainsKey(key))
            {
                listedIssues.Remove(key);
            }

            RefreshProblematicFlag();
        }

        public void ToggleIssueHighlighting(string key, bool showHighlighting)
        {
            if (listedIssues.ContainsKey(key))
            {
                listedIssues[key].HideHighlighting = !showHighlighting;
            }
            RefreshProblematicFlag();
        }

        public void ClearIssues()
        {
            listedIssues.Clear();
            RefreshProblematicFlag();
        }

        public void RefreshProblematicFlag()
        {
            OnProblematicChanged(EventArgs.Empty);
            InvokePropertyChanged(nameof(IsProblematic));
        }

        public bool HasAnyOtherIssues(string currentIssueKey)
        {
            return listedIssues.Any(x => x.Key != currentIssueKey);
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
