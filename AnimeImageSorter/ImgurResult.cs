using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeImageSorter
{
    class ImgurResult
    {
        public string url;
        public int userRate;
        public int clientRate;
        public int postRemaining;
        public DateTimeOffset userReset;
        public int postReset;

        public ImgurResult(string url, int userRate, int clientRate, int postRemaining, DateTimeOffset userReset, int postReset)
        {
            this.url = url;
            this.userRate = userRate;
            this.clientRate = clientRate;
            this.postRemaining = postRemaining;
            this.userReset = userReset;
            this.postReset = postReset;
        }
    }
}
