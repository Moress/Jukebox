using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using IrrKlang;

namespace Jukebox.Server
{
    class Config
    {
        private Config() { 
        // try get config from app.config
            LoadConfig();
        }

        static Config instance;

        public static Config GetInstance()
        {
            if (Config.instance == null)
            {
                Config.instance = new Config();
            }
            return Config.instance;
        }

        // default values
        // votes to next track
        int votesToSkip = 4;
        // path to cache
        string cacheDir = @"C:\temp\jukebox\";
        // user default device
        bool useDefaultDevice = true;
        // Recording device
        string deviceId = "";
        // host
        string host = "net.tcp://localhost:4502/";
        // socketpolicyserver
        bool sspb = true;
        // VKLogin
        string vkLogin = "antoshalee@gmail.com";
        // VKPassword
        string vkPassword = "kvitunov";
        // initial action points
        int initialActionPoints = 4;
        // user manager update milliseconds
        int actionPointsRestoreTime = 60 * 1000 * 10; // 10 min
        // download file timeout
        int downloadTimeout = 60000;
        // play track from cache randomly
        bool playRandomFromCache = true;
        // debug file
        string debugFileName = "debug.txt";

        public int VotesToSkip
        {
            get
            {
                return votesToSkip;
            }
            set
            {
                votesToSkip = value;
            }
        }

        public int InitialActionPoints
        {
            get
            {
                return initialActionPoints;
            }
            set
            {
                initialActionPoints = value;
            }
        }

        public int ActionPointsRestoreTime
        {
            get
            {
                return actionPointsRestoreTime;
            }
            set
            {
                actionPointsRestoreTime = value;
            }
        }

        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
            }
        }

        public bool HasSocketPolicyServer
        {
            get
            {
                return sspb;
            }
            set
            {
                sspb = value;
            }
        }

        public string CacheDir
        {
            get 
            { 
                return cacheDir;
            }
            set
            {
                cacheDir = value;
                if (!Directory.Exists(cacheDir))
                {
                    Directory.CreateDirectory(cacheDir);
                }
            }
        }

        public string DeviceId
        {
            get
            {
                return deviceId;
            }
            set
            {
                deviceId = value;
            }
        }

        public string VKLogin
        {
            get
            {
                return vkLogin;
            }
            set
            {
                vkLogin = value;
            }
        }

        public string VKPassword
        {
            get
            {
                return vkPassword;
            }
            set
            {
                vkPassword = value;
            }
        }

        public int DownloadTimeout
        {
            get
            {
                return downloadTimeout;
            }
            set
            {
                downloadTimeout = value;
            }
        }

        public bool PlayRandomFromCache
        {
            get
            {
                return playRandomFromCache;
            }
            set
            {
                playRandomFromCache = value;
            }
        }


        public string DebugFileName
        {
            get
            {
                return debugFileName;
            }
            set
            {
                debugFileName = value;
            }
        }

        private bool ExistDevice(string deviceId)
        {
            ISoundDeviceList outputDevices = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice);
            for (int i = 0; i < outputDevices.DeviceCount; i++)
            {
                if (outputDevices.getDeviceID(i) == deviceId)
                {
                    return true;
                }
            }
            return false;
        }

        private string DeviceDialog()
        {
            Console.WriteLine("Output devices: ");
            ISoundDeviceList outputDevices = new ISoundDeviceList(SoundDeviceListType.PlaybackDevice);
            for (int i = 0; i < outputDevices.DeviceCount; i++)
            {
                Console.WriteLine(i.ToString() + ": " + outputDevices.getDeviceDescription(i));
            }
            Console.Write("Choose device (0): ");
            var deviceString = Console.ReadLine();
            int deviceIndex = deviceString.Length != 1 ? 0 : int.Parse(deviceString);
            string deviceID = outputDevices.getDeviceID(deviceIndex);
            return deviceID;
        }

        public void HotLoad()
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if ((config.AppSettings.Settings["votesToSkip"] != null) &&
                (config.AppSettings.Settings["votesToSkip"].Value != null) &&
                (config.AppSettings.Settings["votesToSkip"].Value != ""))
            {
                try { VotesToSkip = Convert.ToInt32(config.AppSettings.Settings["votesToSkip"].Value); }
                catch { }
            }
            if ((config.AppSettings.Settings["host"] != null) &&
                (config.AppSettings.Settings["host"].Value != null) &&
                (config.AppSettings.Settings["host"].Value != ""))
            {
                try { Host = @config.AppSettings.Settings["host"].Value; }
                catch { }
            }
            if ((config.AppSettings.Settings["useDefaultDevice"] != null) &&
                (config.AppSettings.Settings["useDefaultDevice"].Value != null) &&
                (config.AppSettings.Settings["useDefaultDevice"].Value != ""))
            {
                try { useDefaultDevice = Convert.ToBoolean(config.AppSettings.Settings["useDefaultDevice"].Value); }
                catch { }
            }
            if ((config.AppSettings.Settings["deviceId"] != null) &&
                (config.AppSettings.Settings["deviceId"].Value != null) &&
                (config.AppSettings.Settings["deviceId"].Value != ""))
            {
                try { DeviceId = config.AppSettings.Settings["deviceId"].Value; }
                catch { }
            }
            if ((config.AppSettings.Settings["cacheDir"] != null) &&
                (config.AppSettings.Settings["cacheDir"].Value != null) &&
                (config.AppSettings.Settings["cacheDir"].Value != ""))
            {
                try { CacheDir = @config.AppSettings.Settings["cacheDir"].Value; }
                catch { }
            }
            if ((config.AppSettings.Settings["sspb"] != null) &&
                (config.AppSettings.Settings["sspb"].Value != null) &&
                (config.AppSettings.Settings["sspb"].Value != ""))
            {
                try { HasSocketPolicyServer = Convert.ToBoolean(config.AppSettings.Settings["sspb"].Value); }
                catch { }
            }
            if ((config.AppSettings.Settings["vkLogin"] != null) &&
                (config.AppSettings.Settings["vklogin"].Value != null) &&
                (config.AppSettings.Settings["vklogin"].Value != ""))
            {
                try { VKLogin = config.AppSettings.Settings["vkLogin"].Value; }
                catch { }
            }
            if ((config.AppSettings.Settings["vkPassword"] != null) &&
                (config.AppSettings.Settings["vkPassword"].Value != null) &&
                (config.AppSettings.Settings["vkPassword"].Value != ""))
            {
                try { VKPassword = config.AppSettings.Settings["vkPassword"].Value; }
                catch { }
            }
            if ((config.AppSettings.Settings["initialActionPoints"] != null) &&
                (config.AppSettings.Settings["initialActionPoints"].Value != null) &&
                (config.AppSettings.Settings["initialActionPoints"].Value != ""))
            {
                try { InitialActionPoints = Convert.ToInt32(config.AppSettings.Settings["initialActionPoints"].Value); }
                catch { }
            }
            if ((config.AppSettings.Settings["actionPointsRestoreTime"] != null) &&
                (config.AppSettings.Settings["actionPointsRestoreTime"].Value != null) &&
                (config.AppSettings.Settings["actionPointsRestoreTime"].Value != ""))
            {
                try { ActionPointsRestoreTime = Convert.ToInt32(config.AppSettings.Settings["actionPointsRestoreTime"].Value); }
                catch { }
            }
            if ((config.AppSettings.Settings["downloadTimeout"] != null) &&
                (config.AppSettings.Settings["downloadTimeout"].Value != null) &&
                (config.AppSettings.Settings["downloadTimeout"].Value != ""))
            {
                try { DownloadTimeout = Convert.ToInt32(config.AppSettings.Settings["downloadTimeout"].Value); }
                catch { }
            }
            if ((config.AppSettings.Settings["playRandomFromCache"] != null) &&
                (config.AppSettings.Settings["playRandomFromCache"].Value != null) &&
                (config.AppSettings.Settings["playRandomFromCache"].Value != ""))
            {
                try { PlayRandomFromCache = Convert.ToBoolean(config.AppSettings.Settings["playRandomFromCache"].Value); }
                catch { }
            }


            // dialog for special devices
            if (!useDefaultDevice)
            {
                if (!ExistDevice(DeviceId) || DeviceId == "")
                {
                    string deviceIdDialogResult = DeviceDialog();
                    Console.WriteLine("Do you want add this device as default for Jukebox? (Yes)");
                    var result = Console.ReadLine().ToUpper();
                    if ((result == "") || (result == null) || (result == "Y") || (result == "YES"))
                    {
                        if (deviceIdDialogResult == "")
                        {
                            config.AppSettings.Settings.Remove("useDefaultDevice");
                            config.AppSettings.Settings.Add("useDefaultDevice", true.ToString());
                        }
                        else
                        {
                            config.AppSettings.Settings.Remove("deviceId");
                            config.AppSettings.Settings.Add("deviceId", deviceIdDialogResult);
                        }
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");
                        DeviceId = deviceIdDialogResult;
                    }
                }
            }
            else
            {
                DeviceId = "";
            }
        }
        
    }
}
