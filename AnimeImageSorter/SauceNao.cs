using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnimeImageSorter
{
    class SauceNao
    {
        private static string ENDPOINT = "https://saucenao.com/search.php";
        private string ApiKey;

        public SauceNao(string apiKey)
        {
            this.ApiKey = apiKey;
        }

        public SauceNaoResult Request(string url)
        {
            SauceNaoResult sauceNaoResult = null;
            using (var web = new WebClient())
            {
                web.QueryString.Add("db", "999");
                web.QueryString.Add("output_type", "2");
                web.QueryString.Add("numres", "16");
                web.QueryString.Add("api_key", ApiKey);
                web.QueryString.Add("url", url);          

                try
                {
                    string response = web.DownloadString(ENDPOINT);
                    JObject jObject = JObject.Parse(response);
                    sauceNaoResult = new SauceNaoResult(jObject);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return sauceNaoResult;
        }
    }
}
