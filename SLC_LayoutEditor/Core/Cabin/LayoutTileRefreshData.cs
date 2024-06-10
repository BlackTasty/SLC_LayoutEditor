using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Cabin
{
    internal class LayoutTileRefreshData
    {
        private readonly bool isLoadingOnly;
        private readonly CabinLayout cabinLayout;

        public bool IsLoadingOnly => isLoadingOnly;

        public CabinLayout CabinLayout => cabinLayout;

        public LayoutTileRefreshData(CabinLayout cabinLayout, bool isLoadingOnly)
        {
            this.isLoadingOnly = isLoadingOnly;
            this.cabinLayout = cabinLayout;
        }
    }
}
