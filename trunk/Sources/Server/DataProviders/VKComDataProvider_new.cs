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

                var url = "http://vk.com/al_search.php";

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(_cookie);
                request.ContentType = @"application/x-www-form-urlencoded";
                request.Method = "POST";

                string parameters = "al=1&c[q]=" + HttpUtility.UrlEncode(query).ToUpper() + "&c[section]=audio";
                byte[] message = Encoding.UTF8.GetBytes(parameters);
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(message, 0, message.Length);
                    requestStream.Close();
                }

                Stream responseStream = request.GetResponse().GetResponseStream();
                string content = "";

                using (var reader = new StreamReader(responseStream, Encoding.GetEncoding(1251)))
                {
                    content = reader.ReadToEnd();
                }



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
                        var track = new Track();
                        audioElement = divTrack;

                        if (audioElement.FirstAttribute.Value == "search_more_results")
                        {
                            break;
                        }

                        var trackUri = audioElement.XPathSelectElement("div/table/tr/td/input").Attribute("value").Value;

                        var terms = trackUri.Replace("\'", "").Split(',');

                        var startId = terms[0].IndexOf("audio/") + "audio/".Length;
                        var finishId = terms[0].IndexOf(".mp3");

                        track.Id = terms[0].Substring(startId, finishId - startId);
                        track.Uri = new Uri(terms[0]);

                        string titleTag = audioElement.XPathSelectElement("div/table/tr/td/div/span").ToString(SaveOptions.DisableFormatting);
                        track.Title = PrepareTitle(titleTag);
                        string singerTag = audioElement.XPathSelectElement("div/table/tr/td/div/b/a").ToString(SaveOptions.DisableFormatting);
                        track.Singer = PrepareTitle(singerTag);

                        var duration = audioElement.XPathSelectElement("div/table/tr/td/div[@class='duration fl_r']").Value;

                        TimeSpan tmp;
                        TimeSpan.TryParse("00:" + duration, out tmp);
                        track.Duration = tmp;

                        track.Source = TrackSource.VK;

                        track.Id = track.GetHash();

                        result.Add(track);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("VKComDataProvider error: " + e.Message);
                Debug.WriteLine("Query: " + query);
            }

            

            return new ReadOnlyCollection<Track>(result);
        }

        string PrepareTitle(string content)
        {
            string patternTwoTags = "</span><span .*?>";
            content = Regex.Replace(content, patternTwoTags, " ");
            string patternTag = "<.*?>";
            content = Regex.Replace(content, patternTag, String.Empty);
            content = content.Trim();
            return content;
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
                WebDownload c = new WebDownload();
                c.Timeout = Config.GetInstance().DownloadTimeout;

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
            HttpWebRequest landingRequest = (HttpWebRequest)System.Net.WebRequest.Create("http://vk.com/");
            landingRequest.Timeout = 100000;
            landingRequest.AllowAutoRedirect = false;
            landingRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";
            var landingResponse = landingRequest.GetResponse();
            var landingResponseString = new StreamReader(landingResponse.GetResponseStream()).ReadToEnd();
            var ipHRegexMatch = Regex.Match(landingResponseString, "<input type=\"hidden\" name=\"ip_h\" value=\"(.+)\"");
            var lgHRegexMatch = Regex.Match(landingResponseString, "<input type=\"hidden\" name=\"lg_h\" value=\"(.+)\"");
            
            var ipH = ipHRegexMatch.Result("$1");
            var lgH = lgHRegexMatch.Result("$1");

            string landingResponseHeaders = landingResponse.Headers.ToString();

            Regex remixlhkRegex = new Regex("remixlhk=([a-z0-9]+); exp");
            var remixlhk = remixlhkRegex.Match(landingResponseHeaders).Groups[1].Value;
            
            HttpWebRequest wrPOSTURL = (HttpWebRequest)System.Net.WebRequest.Create(
                "http://login.vk.com/?act=login" //&email=" + email + "&pass=" + pass + "&lg_h=" + lgH
            );
            wrPOSTURL.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";
            wrPOSTURL.AllowAutoRedirect = false;
            wrPOSTURL.Timeout = 100000;
            wrPOSTURL.Method = "POST";
            wrPOSTURL.Referer = "http://vk.com/";
            wrPOSTURL.Host = "login.vk.com";
            wrPOSTURL.Headers.Add("Accept-Language: ru,en-US;q=0.7,en;q=0.3");
            wrPOSTURL.Headers.Add("DNT: 1");
            wrPOSTURL.Headers.Add("Accept-Encoding: gzip, deflate");
            wrPOSTURL.ServicePoint.Expect100Continue = false;
            
            wrPOSTURL.Accept = "text/html, application/xhtml+xml, */*";
            wrPOSTURL.ContentType = "application/x-www-form-urlencoded";
            wrPOSTURL.CookieContainer = new CookieContainer();
            wrPOSTURL.CookieContainer.Add(new Cookie("remixlang", "0", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixflash", "15.0.0", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixscreen_depth", "24", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixdt", "14400", "/", ".vk.com"));
            wrPOSTURL.CookieContainer.Add(new Cookie("remixtst", "5928f8be", "/", ".vk.com"));
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
                writer.Write("_origin=http%3A%2F%2Fvk.com");
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

    }
}
