using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel;

namespace SLC_LayoutEditor.ViewModel
{
    public class BaseDialogViewModel : ViewModelBase
    {
        public virtual bool IsValid { get; }
    }
}
