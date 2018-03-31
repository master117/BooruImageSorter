using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AnimeImageSorter
{
    class BooruResult
    {
        public JArray jArray;

        public BooruResult(JArray jArray)
        {
            this.jArray = jArray;
        }
    }
}
