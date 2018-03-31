using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeImageSorter
{
    class Program
    {
        //Startup options
        enum Sortby
        {
            Unknown = 0,
            Series = 1,
            Character = 2
        }
        static Sortby CurrentSortBy = Sortby.Unknown;

        enum FileOperation
        {
            Unknown = 0,
            Move = 1,
            Copy = 2
        }
        static FileOperation CurrentFileOperation = FileOperation.Unknown;

        enum MD5Option
        {
            Unknown = 0,
            Hard = 1,
            Soft = 2
        }
        static MD5Option CurrentMD5Option = MD5Option.Unknown;

        enum MultipleOption
        {
            Unknown = 0,
            Copies = 1,
            MixedFolder = 2,
            First = 3,
            Skip = 4
        }
        static MultipleOption CurrentMultipleOption = MultipleOption.Unknown;

        enum ReverseImageSearch
        {
            Unknown = 0,
            Yes = 1,
            No = 2
        }
        static ReverseImageSearch CurrentReverseImageSearch = ReverseImageSearch.Unknown;

        //Regex used to find MD5 in filenames
        private static Regex md5Regex = new Regex("^[0-9a-f]{32}$");

        //SauceNao and Imgur Stuff
        private static string sauceNaoApiKey;
        private static string imgurApiKey;
        private static int remainingSauces = int.MaxValue;
        private static int remainingSaucesLong = int.MaxValue;
        private static ImgurResult lastImage;

        //Base directory for all further operations
        private static string baseDirectory;

        static void Main(string[] args)
        {
            #region Options
            //Should replace this with switches
            // Get Sort Type
            Console.WriteLine("Enter image directory (no trailing / ) or leave clear and just press enter to use current directory:");
            string directory = Console.ReadLine();

            if(directory != "" && !Directory.Exists(directory))
            {
                Console.WriteLine(directory + " is not a valid directory, press any key to quit.");
                Console.ReadKey();
                return;
            }

            if (directory != "")
                baseDirectory = directory;
            else
                baseDirectory = Directory.GetCurrentDirectory();
                

            Console.WriteLine("\nAll operations will work on:\n" + baseDirectory + "\nnot on subdirectories.");
            Console.WriteLine("\nPress the matching letter key to select options:");
            Console.WriteLine("Sort by:\n s(eries) / c(haracter) / q(uit)");
            string key = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key == "S")
                CurrentSortBy = Sortby.Series;

            if (key == "C")
                CurrentSortBy = Sortby.Character;

            if (key == "Q" || CurrentSortBy == Sortby.Unknown)
                return;

            // Get FileOperation Type
            Console.WriteLine("\n\nFile operation:\n m(ove) / c(opy) / q(uit)");
            string key2 = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key2 == "M")
                CurrentFileOperation = FileOperation.Move;

            if (key2 == "C")
                CurrentFileOperation = FileOperation.Copy;

            if (key2 == "Q" || CurrentFileOperation == FileOperation.Unknown)
                return;

            // Get MD5Option Type
            Console.WriteLine("\n\nHash calculation, hard is slower but may be more precise in very rare cases:\n h(ard) / s(oft) / q(uit)");
            string key3 = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key3 == "H")
                CurrentMD5Option = MD5Option.Hard;

            if (key3 == "S")
                CurrentMD5Option = MD5Option.Soft;

            if (key3 == "Q" || CurrentMD5Option == MD5Option.Unknown)
                return;

            // Get MultipleOption Type
            Console.WriteLine("\n\nHow to handle multiple tags/characters/series in the same image:\n c(opies, copy file in multiple folders) / m(ixed, mixed foldernames) / f(irst, first tag) / s(kip) / q(uit)");
            string key4 = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key4 == "C")
                CurrentMultipleOption = MultipleOption.Copies;

            if (key4 == "M")
                CurrentMultipleOption = MultipleOption.MixedFolder;

            if (key4 == "F")
                CurrentMultipleOption = MultipleOption.First;

            if (key4 == "S")
                CurrentMultipleOption = MultipleOption.Skip;

            if (key4 == "Q" || CurrentMultipleOption == MultipleOption.Unknown)
                return;

            // Get ReverseImageSearch Type
            Console.WriteLine("\n\nReverse Image Search images not found through hashing (this is slow and needs extra steps, details in github):\n y(es) / n(o) / q(uit)");
            string key5 = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key5 == "Y")
                CurrentReverseImageSearch = ReverseImageSearch.Yes;

            if (key5 == "N")
                CurrentReverseImageSearch = ReverseImageSearch.No;

            if (key5 == "Q" || CurrentReverseImageSearch == ReverseImageSearch.Unknown)
                return;

            if (CurrentReverseImageSearch == ReverseImageSearch.Yes)
            {
                if (File.Exists("sauceNaoApiKey.txt") && File.Exists("imgurApiKey.txt"))
                {
                    sauceNaoApiKey = File.ReadAllText("sauceNaoApiKey.txt");
                    imgurApiKey = File.ReadAllText("imgurApiKey.txt");
                }
                else
                {
                    Console.WriteLine("\n Either sauceNaoApiKey.txt or imgurApiKey.txt missing. To fix this problem look into github. Press any key to quit.");
                    Console.ReadKey();
                    return;
                }
            }
            #endregion

            // List all files in the current folder
            List<string> files = Directory.EnumerateFiles(baseDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(x => x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) 
                || x.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)).ToList();
            Console.WriteLine("\n Found " + files.Count + " images.");

            //Work all files in the folder
            foreach (var file in files)
            {
                //Extract filename etc
                int filestart = file.LastIndexOf('\\') + 1;
                string filename = file.Substring(filestart, file.LastIndexOf('.') - filestart);
                string filenameLong = file.Substring(filestart);
                Console.WriteLine("\nWorking file: " + filename);
                //Calculate the MD5 Hash for each file
                string md5 = GetMD5(file, filename);

                try
                {
                    //Log
                    Console.WriteLine("Trying Danbooru for file: " + filename + " with hash: " + md5);

                    //Try get JSON Data off file, from hash
                    string danbooruUri = "https://danbooru.donmai.us/posts.json?limit=1&tags=md5:" + md5;
                    var danbooruJson = HttpRequester.GetHttpJSONJArray(danbooruUri);

                    //If booru search didn't find anything, try reverse search
                    if ((danbooruJson == null || danbooruJson.Count == 0) && CurrentReverseImageSearch == ReverseImageSearch.Yes)
                    {
                        #region rateLimits
                        //Adhere to all ratelimits
                        if(remainingSauces < 2)
                        {
                            Console.WriteLine("\nToo many Sauces, approaching SauceNao 20 requests per 30s limit. Waiting 30s.");
                            Thread.Sleep(30000);
                        }

                        if (remainingSaucesLong < 2)
                        {
                            Console.WriteLine("\nToo many Sauces, approaching SauceNao 300 requests per 24hrs limit. Press any key to quit, monitor your usage at https://saucenao.com/user.php?page=search-usage and start again.");
                            Console.ReadKey();
                            return;
                        }

                        if(lastImage.userRate < 15)
                        {
                            Console.WriteLine("\nToo many Imgur uploads. Approaching user rate limit of x per hour. Waiting until user rate is reset at: " + lastImage.userReset.ToLocalTime());
                            Thread.Sleep((int)Math.Ceiling((lastImage.userReset.ToLocalTime() - DateTime.Now).TotalMilliseconds));
                        }

                        if (lastImage.clientRate < 15)
                        {
                            Console.WriteLine("\nToo many Imgur uploads. Approaching client rate limit of 1,250 per day. Press any key to quit and try again in 24hrs.");
                            Console.ReadKey();
                            return;
                        }

                        if (lastImage.postRemaining < 15)
                        {
                            Console.WriteLine("\nToo many Imgur uploads. Approaching post rate limit of 1,250 per hour. Waiting until user rate is reset in: " + lastImage.postReset + " seconds.");
                            Thread.Sleep(lastImage.postReset * 1000);
                        }
                        #endregion

                        Console.WriteLine("Uploading to imgur so it can be used for Reverse Image Search...");
                        ImgurResult image = Imgur.Upload(file, imgurApiKey);
                        lastImage = image;
                        Console.WriteLine("Uploaded: " + Math.Min(lastImage.clientRate, Math.Min(lastImage.userRate, lastImage.postRemaining)) + " upload credits remaining. Reverse Image Searching...");

                        SauceNaoResult response = new SauceNao(sauceNaoApiKey).Request(image.url);
                        var results = response.results;
                        remainingSauces = (int)response.header["short_remaining"];
                        remainingSaucesLong = (int)response.header["long_remaining"];

                        //Remove all low similarity results
                        foreach (var element in results.ToArray().Where(x => (float)x.Key["similarity"] < 90.0))
                            results.Remove(element.Key);

                        //Get danbooru id, if any high similarity result still has one
                        if (results.Any(x => x.Value["danbooru_id"] != null))
                        {
                            Console.WriteLine("Result found.");
                            string danbooruId = results.First(x => x.Value["danbooru_id"] != null).Value["danbooru_id"].ToString();

                            //Get JSON Data on file
                            danbooruUri = "https://danbooru.donmai.us/posts/" + danbooruId + ".json";
                            danbooruJson = new JArray() { HttpRequester.GetHttpJSONJToken(danbooruUri) };
                        }
                        else
                        {
                            Console.WriteLine("No result found.");
                        }
                    }

                    //Work JSON Data
                    if (danbooruJson != null && danbooruJson.Count > 0)
                    {
                        //Log
                        Console.WriteLine("Found file on Danbooru: " + filename);

                        //Create bImage object
                        var bImage = new BImage(danbooruJson[0]);

                        //CopyMove file
                        if (bImage.charCount >= 1 && CurrentSortBy == Sortby.Character
                            || bImage.copyRightCount >= 1 && CurrentSortBy == Sortby.Series)
                        {
                            CopyMoveFile(file, filenameLong, bImage);
                        }
                    }
                    else
                    {
                        Console.WriteLine("File could not be identified.");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("\nAll Operations finished. Press any key to exit.");
            Console.ReadKey();
        }

        private static string GetMD5(string file, string filename)
        {
            //improves speed, may reduce accuracy, when a file has a name that could be its MD5 hash, but isn't
            if (CurrentMD5Option == MD5Option.Soft && md5Regex.IsMatch(filename))
                return filename;
            else
                return CalculateMD5(file);
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static void CopyMoveFile(string file, string fileNameLong, BImage bImage)
        {
            //Determine targetfolder(s)
            List<string> targetFolders = new List<string>();
            switch (CurrentMultipleOption)
            {
                case MultipleOption.Copies:
                    switch (CurrentSortBy)
                    {
                        case Sortby.Character:
                            targetFolders.AddRange(bImage.characters);
                            break;
                        case Sortby.Series:
                            targetFolders.AddRange(bImage.copyRights);
                            break;
                    }
                    break;
                case MultipleOption.MixedFolder:
                    switch (CurrentSortBy)
                    {
                        case Sortby.Character:
                            targetFolders.Add(bImage.charactersString);
                            break;
                        case Sortby.Series:
                            targetFolders.Add(bImage.copyRightsString);
                            break;
                    }
                    break;
                case MultipleOption.First:
                    switch (CurrentSortBy)
                    {
                        case Sortby.Character:
                            targetFolders.Add(bImage.characters.First());
                            break;
                        case Sortby.Series:
                            targetFolders.Add(bImage.copyRights.First());
                            break;
                    }
                    break;
                case MultipleOption.Skip:
                    switch (CurrentSortBy)
                    {
                        case Sortby.Character:
                            if (bImage.charCount > 1)
                                return;

                            targetFolders.Add(bImage.characters.First());
                            break;
                        case Sortby.Series:
                            if (bImage.copyRightCount > 1)
                                return;

                            targetFolders.Add(bImage.copyRights.First());
                            break;
                    }
                    break;
            }

            //CopyMove File in targetfolder(s)
            foreach (var targetFolder in targetFolders)
            {
                string targetFolderLong = baseDirectory + "/" + targetFolder;
                string targetFileLong = targetFolderLong + "/" + fileNameLong;
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolderLong);

                switch (CurrentFileOperation)
                {
                    case FileOperation.Copy:
                        File.Copy(file, targetFileLong, true);
                        Console.WriteLine("Copying: " + file + " to " + targetFileLong);
                        break;

                    //Copy until last, then move
                    case FileOperation.Move:
                        if (targetFolder == targetFolders.Last())
                        {
                            File.Move(file, targetFileLong);
                            Console.WriteLine("Moving: " + file + " to " + targetFileLong);
                        }
                        else
                        {
                            File.Copy(file, targetFileLong, true);
                            Console.WriteLine("Moving: " + file + " to " + targetFileLong);
                        }
                        break;
                }
            }
        }
    }
}
