namespace AnimeImageSorter
{
    internal class Result
    {
        public dynamic header;
        public dynamic data;

        public Result(dynamic header, dynamic data)
        {
            this.header = header;
            this.data = data;
        }
    }
}