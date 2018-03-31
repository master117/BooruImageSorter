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
            WebClient webClient = new WebClient();

            webClient.QueryString.Add("db", "999");
            webClient.QueryString.Add("output_type", "2");
            webClient.QueryString.Add("numres", "16");
            webClient.QueryString.Add("api_key", ApiKey);
            webClient.QueryString.Add("url", url);

            SauceNaoResult sauceNaoResult = null;

            try
            {
                string response = webClient.DownloadString(ENDPOINT);
                JObject jObject =  JObject.Parse(response);
                sauceNaoResult = new SauceNaoResult(jObject);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return sauceNaoResult;
        }
    }
}
