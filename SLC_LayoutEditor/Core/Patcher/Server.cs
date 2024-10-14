﻿using System;
using System.Net;

namespace SLC_LayoutEditor.Core.Patcher
{
    class Server
    {
        public Server(string url)
        {
            URL = url;
        }

        public string URL { get; set; }

        public bool IsAvailable
        {
            get
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    request.AllowAutoRedirect = false;
                    request.Method = WebRequestMethods.Http.Head;
                    using (WebResponse response = request.GetResponse())
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
