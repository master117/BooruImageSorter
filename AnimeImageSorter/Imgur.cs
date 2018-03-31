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
        public static ImgurResult Upload(string path, string apiKey)
        {
            using (var w = new WebClient())
            {
                w.Headers.Add("Authorization: Client-ID " + apiKey);
                var values = new NameValueCollection
                {
                    { "image", Convert.ToBase64String(File.ReadAllBytes(@path)) }
                };

                string response = System.Text.Encoding.UTF8.GetString(w.UploadValues("https://api.imgur.com/3/upload", values));

                string url = ((dynamic)JsonConvert.DeserializeObject(response)).data.link;
                int userRemaining = int.Parse(w.ResponseHeaders.Get("X-RateLimit-UserRemaining"));
                int clientRemaining = int.Parse(w.ResponseHeaders.Get("X-RateLimit-ClientRemaining"));
                int postRemaining = int.Parse(w.ResponseHeaders.Get("X-Post-Rate-Limit-Remaining"));
                DateTimeOffset userReset = DateTimeOffset.FromUnixTimeSeconds(int.Parse(w.ResponseHeaders.Get("X-RateLimit-UserReset")));
                int postReset = int.Parse(w.ResponseHeaders.Get("X-Post-Rate-Limit-Reset"));

                var imgurResult = new ImgurResult(url, userRemaining, clientRemaining, postRemaining, userReset, postReset);

                return imgurResult;
            }
        }
    }
}
