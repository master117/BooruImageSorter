using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnimeImageSorter
{
    class Imgur
    {
        private static string ENDPOINT = "https://api.imgur.com/3/upload";

        public static ImgurResult Upload(string path, string apiKey)
        {
            ImgurResult imgurResult = null;

            using (var web = new WebClient())
            {
                web.Headers.Add("Authorization: Client-ID " + apiKey);
                var values = new NameValueCollection
                {
                    { "image", Convert.ToBase64String(File.ReadAllBytes(@path)) }
                };

                try
                {

                    string response = System.Text.Encoding.UTF8.GetString(web.UploadValues(ENDPOINT, values));

                    string url = ((dynamic)JsonConvert.DeserializeObject(response)).data.link;
                    int userRemaining = int.Parse(web.ResponseHeaders.Get("X-RateLimit-UserRemaining"));
                    int clientRemaining = int.Parse(web.ResponseHeaders.Get("X-RateLimit-ClientRemaining"));
                    int postRemaining = int.Parse(web.ResponseHeaders.Get("X-Post-Rate-Limit-Remaining"));
                    DateTimeOffset userReset = DateTimeOffset.FromUnixTimeSeconds(int.Parse(web.ResponseHeaders.Get("X-RateLimit-UserReset")));
                    int postReset = int.Parse(web.ResponseHeaders.Get("X-Post-Rate-Limit-Reset"));

                    imgurResult = new ImgurResult(url, userRemaining, clientRemaining, postRemaining, userReset, postReset);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }              
            }

            return imgurResult;
        }
    }
}
