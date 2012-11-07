using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;

namespace Jukebox.Server.DataProviders
{
    public class WebDownload : WebClient
    {
        private int _timeout;
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public WebDownload()
        {
            this._timeout = 60000;
        }

        public WebDownload(int timeout)
        {
            this._timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = this._timeout;
            return result;
        }
    }
}
