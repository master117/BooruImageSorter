using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

        //Regex used to find MD5 in filenames
        private static Regex md5Regex = new Regex("^[0-9a-f]{32}$");

        static void Main(string[] args)
        {
            //Should rpleace this with switches
            // Get Sort Type
            Console.WriteLine("All operation will work on: " + Directory.GetCurrentDirectory() + " not on subdirectories.");
            Console.WriteLine("Sort by: s(eries) / c(haracter) / q(uit)");
            string key = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key == "S")
                CurrentSortBy = Sortby.Series;

            if (key == "C")
                CurrentSortBy = Sortby.Character;

            if (key == "Q" || CurrentSortBy == Sortby.Unknown)
                return;

            // Get FileOperation Type
            Console.WriteLine("\n\nFile operation: m(ove) / c(opy) / q(uit)");
            string key2 = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key2 == "M")
                CurrentFileOperation = FileOperation.Move;

            if (key2 == "C")
                CurrentFileOperation = FileOperation.Copy;

            if (key2 == "Q" || CurrentFileOperation == FileOperation.Unknown)
                return;

            // Get MD5Option Type
            Console.WriteLine("\n\nHash calculation, hard is slower but may be more precise in very rare cases: h(ard) / s(oft) / q(uit)");
            string key3 = Console.ReadKey().KeyChar.ToString().ToUpper();

            if (key3 == "H")
                CurrentMD5Option = MD5Option.Hard;

            if (key3 == "S")
                CurrentMD5Option = MD5Option.Soft;

            if (key3 == "Q" || CurrentMD5Option == MD5Option.Unknown)
                return;

            // Get MultipleOption Type
            Console.WriteLine("\n\nHow to handle multiple tags/characters/series in the same imagetags:\nc(opies, copy file in multiple folders) / m(ixed, mixed foldernames) / f(irst, first tag) / s(kip) / q(uit)");
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

            // List all files in the current folder
            List<string> files = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.TopDirectoryOnly)
                .Where(x => x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) 
                || x.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)).ToList();

            //Work all files in the folder
            foreach (var file in files)
            {
                //Extract filename etc
                int filestart = file.LastIndexOf('\\') + 1;
                string filename = file.Substring(filestart, file.LastIndexOf('.') - filestart);
                string filenameLong = file.Substring(filestart);

                //Calculate the MD5 Hash for each file
                string md5 = GetMD5(file, filename);

                try
                {
                    //Log
                    Console.WriteLine(md5);
                    Console.WriteLine("Trying Danbooru for file: " + filename);

                    //Get JSON Data on file
                    string danbooruUri = "https://danbooru.donmai.us/posts.json" + "?limit=1" + "&tags=md5:" + md5;
                    var danbooruJson = HttpRequester.GetHttpJSONJArray(danbooruUri);

                    //Work JSON Data
                    if(danbooruJson != null && danbooruJson.Count > 0)
                    {
                        //Log
                        Console.WriteLine("Found file on Danbooru: " + filename);

                        //Create bImage object
                        var bImage = new BImage(danbooruJson);

                        //CopyMove file
                        if(bImage.charCount >= 1 && CurrentSortBy == Sortby.Character 
                            || bImage.copyRightCount >= 1 && CurrentSortBy == Sortby.Series)
                        {
                            CopyMoveFile(file, filenameLong, bImage);
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }                
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
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                switch (CurrentFileOperation)
                {
                    case FileOperation.Copy:
                        File.Copy(file, targetFolder + "/" + fileNameLong, true);
                        break;

                    //Copy until last, then move
                    case FileOperation.Move:
                        if(targetFolder == targetFolders.Last())
                            File.Move(file, targetFolder + "/" + fileNameLong);
                        else
                            File.Copy(file, targetFolder + "/" + fileNameLong, true);
                        break;
                }
            }
        }
    }
}
