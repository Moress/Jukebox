using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Jukebox.Client2.JukeboxService;

namespace Jukebox.Client2.Misc
{
    public class IsolatedStorageManager
    {
        public static void SetKeyValue(string key, object value)
        {
            string valueString = Serialize(value);
            IsolatedStorageSettings.SiteSettings.Remove(key);
            IsolatedStorageSettings.SiteSettings.Add(key, valueString);
            IsolatedStorageSettings.SiteSettings.Save();
        }

        public static object GetValueByKey(string key)
        {
            if (IsolatedStorageSettings.SiteSettings.Contains(key))
            {
                string xmlValue = (string)IsolatedStorageSettings.SiteSettings[key];
                return Deserialize<object>(xmlValue);
            }

            return null;
        }

        private static List<Type> knownTypeList = new List<Type> { typeof(TrackSourceComboItem),
                                                                   typeof(ObservableCollection<TrackSourceComboItem>)};

        public static string Serialize<T>(T data)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T), knownTypeList);
                
                serializer.WriteObject(memoryStream, data);

                memoryStream.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(memoryStream);
                string content = reader.ReadToEnd();
                return content;
            }
        }

        public static T Deserialize<T>(string xml)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                var serializer = new DataContractSerializer(typeof(T), knownTypeList);
                T theObject = (T)serializer.ReadObject(stream);
                return theObject;
            }
        }
    }
}
