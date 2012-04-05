using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jukebox.Server
{
    class Context
    {
        // Path to cache
        string cacheDir;
        public string CacheDir
        {
            get 
            { 
                return cacheDir;
            }
            set
            {
                if (cacheDir == null)
                {
                    cacheDir = value;
                }
            }
        }

        // Recording device
        string deviceId;
        public string DeviceId
        {
            get
            {
                return deviceId;
            }
            set
            {
                if (deviceId == null)
                {
                    deviceId = value;
                }
            }
        }

        static Context instance;

        private Context() { }

        public static Context GetInstance()
        {
            if (Context.instance == null)
            {
                Context.instance = new Context();
            }
            return Context.instance;
        }
    }
}
