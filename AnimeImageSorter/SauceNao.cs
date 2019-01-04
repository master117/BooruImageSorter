using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public SauceNaoResult Request(Stream file)
        {
            try
            {
                // Start the HandleFile method.
                Task<string> task = RequestFromFile(file);
                task.Wait();
                string response = task.Result;
                JObject jObject = JObject.Parse(response);
                SauceNaoResult sauceNaoResult = sauceNaoResult = new SauceNaoResult(jObject);

                return sauceNaoResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> RequestFromFile(Stream file)
        {
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent("9"), "db");
            form.Add(new StringContent("2"), "output_type");
            form.Add(new StringContent("16"), "numres");
            form.Add(new StringContent(ApiKey), "api_key");
            form.Add(new StreamContent(file), "file", "file.jpg");

            HttpResponseMessage response = await httpClient.PostAsync("https://saucenao.com/search.php", form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = response.Content.ReadAsStringAsync().Result;

            return sd;
        }
    }
}
