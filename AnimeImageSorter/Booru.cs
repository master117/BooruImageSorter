using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnimeImageSorter
{
    class Booru
    {
        private static string ENDPOINTMD5 = "https://danbooru.donmai.us/posts.json";
        private static string ENDPOINTID = "https://danbooru.donmai.us/posts/";

        public BooruResult GetFromMD5(string md5)
        {
            BooruResult booruResult = null;

            using (var web = new WebClient())
            {
                web.QueryString.Add("limit", "1");
                web.QueryString.Add("tags", "md5:" + md5);

                try
                {
                    string response = web.DownloadString(ENDPOINTMD5);
                    JArray jArray = JArray.Parse(response);
                    booruResult = new BooruResult(jArray);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return booruResult;
        }

        public BooruResult GetFromID(string id)
        {
            BooruResult booruResult = null;

            using (var web = new WebClient())
            {
                try
                {
                    string response = web.DownloadString(ENDPOINTID + id + ".json");
                    JArray jArray = new JArray() { JToken.Parse(response) };
                    booruResult = new BooruResult(jArray);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return booruResult;
        }
    }
}
