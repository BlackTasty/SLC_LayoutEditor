using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.Patcher
{
    static class ServerList
    {
        public static readonly List<Server> DEFAULT_SERVERS = new List<Server>()
        {
#if DEBUG
            new Server("http://localhost/slc_le/"), 
#endif
            new Server("http://tasty-apps.bplaced.net/patch/slc_le/"),
            new Server("https://vibrance.lima-city.de/slc_le/")
        };
    }
}
