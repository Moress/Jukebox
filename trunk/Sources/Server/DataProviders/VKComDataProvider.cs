using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Jukebox.Server.Models;
using System.Collections.ObjectModel;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Jukebox.Server.DataProviders
{
    class VKComDataProvider : IDataProvider
    {
        static Cookie _cookie = null;

        const string LOGIN = "antoshalee@gmail.com";
        const string PASSWORD = "kvitunov";

        public IList<Track> Search(string query)
        {
            var result = new List<Track>();
            try
            {
                Ping pinger = new Ping();
                PingOptions pingerOptions = new PingOptions(255, true);
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 2000;
                string hostName = @"vk.com";
                PingReply reply = pinger.Send(hostName, timeout, buffer, pingerOptions);
                if (reply.Status != IPStatus.Success)
                {
                    return new ReadOnlyCollection<Track>(result);
                }

                if (_cookie == null)
                {
                    var res = Auth(LOGIN, PASSWORD, out _cookie);
                    if (!res)
                    {
                        throw new Exception("Failed to authorize at vk.com.");
                    }
                }

                var url = "http://vk.com/gsearch.php?section=audio&q=" + HttpUtility.UrlEncode(query) + "&name=1";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(_cookie);

                var responseStream = request.GetResponse().GetResponseStream();
                string content = "";

                using (var reader = new StreamReader(responseStream, Encoding.GetEncoding(1251)))
                {
                    content = reader.ReadToEnd();
                }

                Regex tableRegex = new Regex(@"(<table id=""searchResults""(.|\n)*>(.|\n)*)<td id=""filters""", RegexOptions.Multiline);

                content = tableRegex.Match(content).Groups[1].Value;
                content = StripHtmlEntities(content) + "</tr></table>";

                // Нет метки о том, что ничего не найдено
                if (!content.Contains("noResults"))
                {
                    var doc = XDocument.Load(new StringReader(content));
                    var td = doc.Root.XPathSelectElement("tr/td");
                    XElement tmpElement;
                    Regex onclickRegex = new Regex(@"\((.*)\)");

                    foreach (var div in td.Elements())
                    {
                        var track = new Track();

                        string onImgClick = div.XPathSelectElement("table/tbody/tr/td/img").Attribute("onclick").Value;
                        var match = onclickRegex.Match(onImgClick);

                        if (!match.Success)
                            continue;

                        var terms = match.Groups[1].Value.Replace("\'", "").Split(',');

                        track.Id = terms[3];
                        track.Uri = new Uri("http://cs" + terms[1] + ".vkontakte.ru/u" + terms[2] + "/audio/" +
                            terms[3] + ".mp3");

                        tmpElement = div.XPathSelectElements("table/tbody/tr/td/div").
                            Where(el => el.Attribute("class").Value == "audioTitle").First();

                        track.Singer = tmpElement.XPathSelectElement("b").Value;

                        track.Title = (from element in tmpElement.Elements("span")
                                       where (element.Attribute("id") != null) && element.Attribute("id").Value.Contains("title")
                                       select element.Value).First();

                        var duration = (from element in tmpElement.Parent.Elements("div")
                                        where element.Attribute("class").Value == "duration"
                                        select element.Value).First();

                        TimeSpan tmp;
                        TimeSpan.TryParse("00:" + duration, out tmp);
                        track.Duration = tmp;
                        track.Source = TrackSource.VK;

                        result.Add(track);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("VKComDataProvider error: " + e.Message);
                Console.WriteLine("Query: " + query);
            }

            return new ReadOnlyCollection<Track>(result);
        }

        string StripHtmlEntities(string content)
        {
            // TODO: 
            //('&#34;','&#38;','&#38;','&#60;','&#62;','&#160;','&#161;','&#162;','&#163;','&#164;','&#165;','&#166;','&#167;','&#168;','&#169;','&#170;','&#171;','&#172;','&#173;','&#174;','&#175;','&#176;','&#177;','&#178;','&#179;','&#180;','&#181;','&#182;','&#183;','&#184;','&#185;','&#186;','&#187;','&#188;','&#189;','&#190;','&#191;','&#192;','&#193;','&#194;','&#195;','&#196;','&#197;','&#198;','&#199;','&#200;','&#201;','&#202;','&#203;','&#204;','&#205;','&#206;','&#207;','&#208;','&#209;','&#210;','&#211;','&#212;','&#213;','&#214;','&#215;','&#216;','&#217;','&#218;','&#219;','&#220;','&#221;','&#222;','&#223;','&#224;','&#225;','&#226;','&#227;','&#228;','&#229;','&#230;','&#231;','&#232;','&#233;','&#234;','&#235;','&#236;','&#237;','&#238;','&#239;','&#240;','&#241;','&#242;','&#243;','&#244;','&#245;','&#246;','&#247;','&#248;','&#249;','&#250;','&#251;','&#252;','&#253;','&#254;','&#255;'); 
            //('&quot;','&amp;','&amp;','&lt;','&gt;','&nbsp;','&iexcl;','&cent;','&pound;','&curren;','&yen;','&brvbar;','&sect;','&uml;','&copy;','&ordf;','&laquo;','&not;','&shy;','&reg;','&macr;','&deg;','&plusmn;','&sup2;','&sup3;','&acute;','&micro;','&para;','&middot;','&cedil;','&sup1;','&ordm;','&raquo;','&frac14;','&frac12;','&frac34;','&iquest;','&Agrave;','&Aacute;','&Acirc;','&Atilde;','&Auml;','&Aring;','&AElig;','&Ccedil;','&Egrave;','&Eacute;','&Ecirc;','&Euml;','&Igrave;','&Iacute;','&Icirc;','&Iuml;','&ETH;','&Ntilde;','&Ograve;','&Oacute;','&Ocirc;','&Otilde;','&Ouml;','&times;','&Oslash;','&Ugrave;','&Uacute;','&Ucirc;','&Uuml;','&Yacute;','&THORN;','&szlig;','&agrave;','&aacute;','&acirc;','&atilde;','&auml;','&aring;','&aelig;','&ccedil;','&egrave;','&eacute;','&ecirc;','&euml;','&igrave;','&iacute;','&icirc;','&iuml;','&eth;','&ntilde;','&ograve;','&oacute;','&ocirc;','&otilde;','&ouml;','&divide;','&oslash;','&ugrave;','&uacute;','&ucirc;','&uuml;','&yacute;','&thorn;','&yuml;'); 

            // lp:708055
            //content = content.Replace((char)0x03, ' ');
            //content = content.Replace((char)0x1F, ' ');

            for (char c = (char)0x00; c < 0x20; c++)
                content = content.Replace(c, ' ');

            Regex regex = new Regex(@"&#(\d+);");
            content = regex.Replace(content, @"^$^#$1;");

            content = content.Replace("&amp;", "^$^amp;");
            content = content.Replace("&nbsp;", "^$^#160;");

            content = content.Replace('&', ' ');

            content = content.Replace("^$^", "&");

            return content;
        }

        public byte[] Download(Track track)
        {
            try
            {
                WebClient c = new WebClient();
                return c.DownloadData(track.Uri);
            }
            catch (Exception e)
            {
                Console.WriteLine("VKComDataProvider error: " + e.Message);
            }
            return null;
        }

        private bool Auth(String email, String pass, out Cookie cookie)
        {
            HttpWebRequest wrGETURL = (HttpWebRequest)System.Net.WebRequest.Create(
                "http://vk.com/login.php?m=1&email=" + email + "&pass=" + pass
            );
            wrGETURL.AllowAutoRedirect = false;
            wrGETURL.Timeout = 100000;
            string headers = wrGETURL.GetResponse().Headers.ToString();

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)wrGETURL.GetResponse();
            StreamReader myStreamReadermy = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(1251));
            string page = myStreamReadermy.ReadToEnd();

            Regex sidregex = new Regex("sid=([a-z0-9]+); exp");
            Match ssid = sidregex.Match(headers);
            string sid = ssid.Groups[1].Value;
            cookie = new Cookie("remixsid", sid, "/", ".vk.com");

            if (String.IsNullOrEmpty(sid))
            {
                return false;
            }
            return true;
        }

    }
}
