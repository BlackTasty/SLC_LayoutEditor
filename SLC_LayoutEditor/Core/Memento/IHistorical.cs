﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Memento
{
    public interface IHistorical<T>
    {
        void RecordChange(T oldValue, T newValue);
    }
}
