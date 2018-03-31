using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AnimeImageSorter
{
    internal class SauceNaoResult
    {
        public JToken header;
        public Dictionary<JToken, JToken> results = new Dictionary<JToken, JToken>();      

        public SauceNaoResult(JObject response)
        {
            this.header = response["header"];

            foreach (var result in response["results"])
            {
                results.Add(result["header"], result["data"]);
            }
        }
    }
}