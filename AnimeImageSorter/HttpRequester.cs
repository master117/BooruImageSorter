using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace AnimeImageSorter
{
    internal class HttpRequester
    {
        public static JArray GetHttpJSONJArray(string adress)
        {
            //first we get a Stream of the Json, for that we use a webrequest
            Stream resStream = GetHttpStream(adress, null, null, DecompressionMethods.None);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            try
            {
                var rawJson = new StreamReader(resStream).ReadToEnd();
                //turns our raw string into a key value lookup
                var json = JArray.Parse(rawJson);

                return json;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }

        public static JToken GetHttpJSONJToken(string adress)
        {
            //first we get a Stream of the Json, for that we use a webrequest
            Stream resStream = GetHttpStream(adress, null, null, DecompressionMethods.None);
            //GetHttpStream may return null if we got no nice answer
            if (resStream == null)
                return null;

            try
            {
                var rawJson = new StreamReader(resStream).ReadToEnd();
                //turns our raw string into a key value lookup
                var json = JToken.Parse(rawJson);

                return json;
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }

        //This method returns a stream on the response of a RESTful webrequest
        private static Stream GetHttpStream(string adress, string username, string password,
            DecompressionMethods decompressionMethod)
        {
            try
            {
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(adress);
                if (username != null)
                    request.Credentials = new NetworkCredential(username, password);

                //request.Referer = "http://www.google.com";
                request.AllowAutoRedirect = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
                request.AutomaticDecompression = decompressionMethod;

                // execute the request
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();

                if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Forbidden)
                    return null;

                // we will read data via the response stream
                return response.GetResponseStream();
            }
            catch (WebException e)
            {
                Console.Write("WebError");
                Console.WriteLine(e.Response);
                Console.WriteLine(e.StackTrace);
                throw new Exception();
            }

            //if we get no response for whatever reason we return null
            return null;
        }
    }
}

