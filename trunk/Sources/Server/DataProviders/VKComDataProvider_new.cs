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
using Jint;

namespace Jukebox.Server.DataProviders
{
    class VKComDataProvider_new : IDataProvider
    {
        static Cookie _cookie = null;

        public TrackSource GetSourceType()
        {
            return TrackSource.VK;
        }

        public IList<Track> Search(string query)
        {
            var result = new List<Track>();
            try
            {
                if (_cookie == null)
                {
                    var res = Auth(Config.GetInstance().VKLogin, Config.GetInstance().VKPassword, out _cookie);
                    if (!res)
                    {
                        throw new Exception("Failed to authorize at vk.com.");
                    }
                }

                string url = "http://vk.com/al_search.php";
                string parameters = "al=1&c[q]=" + HttpUtility.UrlEncode(query).ToUpper() + "&c[section]=audio";

                string content = MakeRequest(url, parameters, _cookie);
                
                string techInfo = @"(<!--.*<!>|<!--.*->|<!>)";
                content = Regex.Replace(content, techInfo, String.Empty);

                // Нет метки о том, что ничего не найдено
                if (content != "")
                {
                    content = "<div>" + content + "</div>";
                    content = StripHtmlEntities(content);
                    var doc = XDocument.Load(new StringReader(content));
                    XElement audioElement;
                    content = null;
                    var elements = doc.Root.Elements();

                    foreach (var divTrack in elements)
                    {
                        try
                        {
                            var track = new Track();
                            audioElement = divTrack;

                            if (audioElement.FirstAttribute.Value == "search_more_results")
                            {
                                break;
                            }

                            var audioData = audioElement.Attribute("data-audio").Value.Split(new string[] { " quot;," }, StringSplitOptions.None);
                            track.Uri = new Uri(audioElement.Attribute("data-full-id").Value, UriKind.Relative);
                            track.Title = PrepareDataLine(audioData[3]);
                            track.Singer = PrepareDataLine(audioData[4]);
                            track.Duration = TimeSpan.FromSeconds(Convert.ToDouble(audioData[5].Split(',')[0]));
                            track.Source = TrackSource.VK;

                            track.Id = track.GetHash();

                            result.Add(track);
                        }
                        catch (Exception innerE)
                        {
                            Debug.WriteLine("VKComDataProvider (track parse): " + innerE.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("VKComDataProvider (global): " + e.Message);
                Debug.WriteLine("Query: " + query);
            }

            

            return new ReadOnlyCollection<Track>(result);
        }

        string PrepareDataLine(string content)
        {
            content = content.Replace("quot;", "");
            content = content.Trim();
            return content;
        }

        string MakeRequest(string url, string parameters, Cookie cookie, string method = "POST")
        {
            Stream responseStream = MakeRequestStream(url, parameters, cookie, method);
            string content = "";

            using (var reader = new StreamReader(responseStream, Encoding.GetEncoding(1251)))
            {
                content = reader.ReadToEnd();
            }

            return content;
        }

        Stream MakeRequestStream(string url, string parameters, Cookie cookie, string method = "POST")
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookie);
            request.ContentType = @"application/x-www-form-urlencoded";
            request.Method = method;

            byte[] message = Encoding.UTF8.GetBytes(parameters);
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(message, 0, message.Length);
                requestStream.Close();
            }

            return request.GetResponse().GetResponseStream();
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
                string url = "http://vk.com/al_audio.php";
                string parameters = "act=reload_audio&al=1&ids=" + HttpUtility.UrlEncode(track.Uri.OriginalString);

                string content = MakeRequest(url, parameters, _cookie);

                var trackUrl = content.Split(new string[] { "<!json>[[" }, StringSplitOptions.None)[1].Split(',')[2];
                trackUrl = Regex.Unescape(trackUrl).Replace("\"", "");
                Jint.Engine engine = new Engine()
                    .Execute(getRevealScript());
                trackUrl = engine.Invoke("reveal", trackUrl).ToString();
                track.Uri = new Uri(trackUrl);

                WebDownload downloader = new WebDownload();
                downloader.Timeout = Config.GetInstance().DownloadTimeout;

                Stream trackStream = downloader.OpenRead(track.Uri);
                MemoryStream resultStream = new MemoryStream();

                byte[] buffer = new byte[4096];
                while (trackStream.CanRead)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    int bytesRead = trackStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    resultStream.Write(buffer, 0, bytesRead);
                }

                return resultStream.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("VKComDataProvider error: " + e.Message);
            }
            return null;
        }

        private bool Auth(String email, String pass, out Cookie cookie)
        {
            HttpWebRequest landingRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://vk.com/");
            landingRequest.Timeout = 100000;
            landingRequest.AllowAutoRedirect = false;
            landingRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            var landingResponse = landingRequest.GetResponse();
            var landingResponseString = new StreamReader(landingResponse.GetResponseStream()).ReadToEnd();
            var ipHRegexMatch = Regex.Match(landingResponseString, "<input type=\"hidden\" name=\"ip_h\" value=\"(.+)\"");
            var lgHRegexMatch = Regex.Match(landingResponseString, "<input type=\"hidden\" name=\"lg_h\" value=\"(.+)\"");
            
            var ipH = ipHRegexMatch.Result("$1");
            var lgH = lgHRegexMatch.Result("$1");

            string landingResponseHeaders = landingResponse.Headers.ToString();

            Regex remixlhkRegex = new Regex("remixlhk=([a-z0-9]+); exp");
            var remixlhk = remixlhkRegex.Match(landingResponseHeaders).NextMatch().Groups[1].Value;
            
            HttpWebRequest wrPOSTURL = (HttpWebRequest)System.Net.WebRequest.Create(
                "http://login.vk.com/?act=login" //&email=" + email + "&pass=" + pass + "&lg_h=" + lgH
            );
            wrPOSTURL.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            wrPOSTURL.AllowAutoRedirect = false;
            wrPOSTURL.Timeout = 100000;
            wrPOSTURL.Method = "POST";
            wrPOSTURL.Referer = "https://vk.com/";
            wrPOSTURL.Host = "login.vk.com";
            wrPOSTURL.Headers.Add("Accept-Language: ru,en-US;q=0.7,en;q=0.3");
            wrPOSTURL.Headers.Add("DNT: 1");
            wrPOSTURL.Headers.Add("Accept-Encoding: gzip, deflate");
            wrPOSTURL.ServicePoint.Expect100Continue = false;
            
            wrPOSTURL.Accept = "text/html, application/xhtml+xml, */*";
            wrPOSTURL.ContentType = "application/x-www-form-urlencoded";
            wrPOSTURL.CookieContainer = new CookieContainer();
            wrPOSTURL.CookieContainer.Add(new Cookie("remixlang", "0", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixrefkey", "b99ef30c41aa2e48fc", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("_ym_uid", "1463544547390306689", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixstid", "1592289695_fc0ec279667da8de94", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("_ym_isad", "2", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixflash", "22.0.0", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixscreen_depth", "24", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixdt", "14400", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixtst", "457fbc51", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixseenads", "0", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("t", "1f6850662e05f3b274c1a56f", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixlhk", remixlhk, "/", ".vk.com"));
            
            using (var writer = new StreamWriter(wrPOSTURL.GetRequestStream()))
            {
                writer.Write("act=login&");
                writer.Write("role=al_frame&");
	            writer.Write("ip_h="+ipH+"&");
	            writer.Write("lg_h="+lgH+"&");
                writer.Write("email=" + email + "&");
                writer.Write("pass=" + pass + "&");
	            writer.Write("expire=&");
	            writer.Write("captcha_sid=&");
	            writer.Write("captcha_key=&");
                writer.Write("_origin=https%3A%2F%2Fvk.com");
	        }
            
            string location = wrPOSTURL.GetResponse().Headers["Location"];

            HttpWebRequest redirectRequest = (HttpWebRequest)System.Net.WebRequest.Create(location);
            redirectRequest.AllowAutoRedirect = false;
            redirectRequest.Timeout = 100000;
            string redirectHeaders = redirectRequest.GetResponse().Headers.ToString();

            Regex sidregex = new Regex("remixsid=([a-z0-9]+); exp");
            Match ssid = sidregex.Match(redirectHeaders);
            string sid = ssid.Groups[1].Value;
            cookie = new Cookie("remixsid", sid, "/", ".vk.com");

            if (String.IsNullOrEmpty(sid))
            {
                return false;
            }
            return true;
        }

        private String getRevealScript()
        {
            return @"var res = function(t, i) { 
                  ""use strict""; 

                  function e(t) {
                   if (~t.indexOf(""audio_api_unavailable"")) {
                    var i = t.split(""?extra="")[1].split(""#""),
                     e = o(i[1]);
                    if (i = o(i[0]), !e || !i) return t;
                    e = e.split(String.fromCharCode(9));
                    for (var a, r, l = e.length; l--;) {
                     if (r = e[l].split(String.fromCharCode(11)), a = r.splice(0, 1, i)[0], !s[a]) return t;
                     i = s[a].apply(null, r)
                    }
                    if (i && ""http"" === i.substr(0, 4)) return i
                   }
                   return t
                  }

                  function o(t) {
                   if (!t || t.length % 4 == 1) return !1;
                   for (var i, e, o = 0, s = 0, r = """"; e = t.charAt(s++);){ 
                    e = a.indexOf(e), ~e && 
                    (i = o % 4 ? 64 * i + e : e, o++ % 4) && (r += String.fromCharCode(255 & i >> (-2 * o & 6)));

                   }
                   return r
                  }
  
                  var a = ""abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMN0PQRSTUVWXYZO123456789+/="",
                   s = {
                    v: function(t) {
                     return t.split("""").reverse().join("""")
                    },
                    r: function(t, i) {
                     t = t.split("""");
                     for (var e, o = a + a, s = t.length; s--;) e = o.indexOf(t[s]), ~e && (t[s] = o.substr(e - i, 1));
                     return t.join("""")
                    },
                    x: function(t, i) {
                     var e = [];
                     i = i.charCodeAt(0);
                     var temp_arr = t.split("""");
                     for (var temp_idx=0;temp_idx < temp_arr.length; temp_idx++) {
                      var elem = temp_arr[temp_idx];
                      e.push(String.fromCharCode(elem.charCodeAt(0) ^ i));
                     }
                     return e.join("""");
                    }
                  }

                  return e;
                }
                var reveal = res();";
        }

    }
}
