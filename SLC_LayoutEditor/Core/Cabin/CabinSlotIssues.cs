using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.Core.Cabin
{
    internal class CabinSlotIssues : ViewModelBase
    {
        private Dictionary<string, CabinSlotIssue> listedIssues = new Dictionary<string, CabinSlotIssue>();

        public bool IsProblematic => listedIssues.Any(x => x.Value.IsProblematic);

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

            InvokePropertyChanged(nameof(IsProblematic));
        }

        public void ToggleIssueHighlighting(string key, bool hideHighlighting)
        {
            if (listedIssues.ContainsKey(key))
            {
                listedIssues[key].HideHighlighting = hideHighlighting;
            }
            InvokePropertyChanged(nameof(IsProblematic));
        }
    }
}
