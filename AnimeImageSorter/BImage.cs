using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeImageSorter
{
    class BImage
    {
        public int charCount;
        public List<string> characters;
        public string charactersString;

        public int copyRightCount;     
        public List<string> copyRights;
        public string copyRightsString;

        public bool SFW = false;

        public BImage(JToken jToken)
        {
            charCount = (int)jToken["tag_count_character"];
            charactersString = ((string)jToken["tag_string_character"]);
            characters = charactersString.Split(' ').ToList();

            copyRightCount = (int)jToken["tag_count_copyright"];
            copyRightsString = ((string)jToken["tag_string_copyright"]);
            copyRights = copyRightsString.Split(' ').ToList();

            string rating = (string)jToken["rating"];

            switch (rating)
            {
                case "s":
                    SFW = true;
                    break;
                case "q":
                case "e":
                default:
                    break;
            }
        }
    }
}
