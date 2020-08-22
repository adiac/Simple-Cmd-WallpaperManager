using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WallpaperManager
{
    static class WallpaperManager
    {
        #region Lists

        public static List<WallpaperType> Types { get; }
        public static List<WallpaperFranchise> Franchises { get; }
        public static List<Wallpaper> Wallpapers { get; }
        public static List<string> AllowedFileExtensions { get; }
        public static List<string> PossibleFileSuffix { get; }


        #endregion

        #region Directories

        public static DirectoryInfo WallpaperDirectory { get; } = new DirectoryInfo("F:/Bilder/Wallpapers/Desktop");
        public static DirectoryInfo OriginalWallpaperDirectory { get; } = new DirectoryInfo("F:/Bilder/Wallpapers/Original");
        //Be careful! This next directory gets cleared without any warning!
        public static DirectoryInfo MergedWallpaperDirectory { get; } = new DirectoryInfo("F:/Bilder/Wallpapers/Desktop-Merge");
        public static DirectoryInfo WorkingDirectoryNamed { get; } = new DirectoryInfo("F:/Bilder/Wallpapers/Workspace/0 Working/3 named");
        public static DirectoryInfo WorkingDirectory { get; } = new DirectoryInfo("F:/Bilder/Wallpapers/Workspace/0 Working");
        public static DirectoryInfo TrashDirectory { get; } = new DirectoryInfo("F:/Bilder/Wallpapers/Sorted out");

        #endregion

        public const string EditedFilesRegexString = @"(?:(?:_cut)|(?:_new))+\.(?:(?:png)|(?:jpg))$";

        #region C'Tor

        static WallpaperManager()
        {
            Franchises = new List<WallpaperFranchise>();
            Types = new List<WallpaperType>();
            Wallpapers = new List<Wallpaper>();
            AllowedFileExtensions = new List<string>();
            AllowedFileExtensions.Add(".png");
            AllowedFileExtensions.Add(".jpg");
            AllowedFileExtensions.Add(".jpeg");

            PossibleFileSuffix.Add("_new");
            PossibleFileSuffix.Add("_cut");

            if (!WallpaperDirectory.Exists)
                throw new DirectoryNotFoundException();
            if (!OriginalWallpaperDirectory.Exists)
                throw new DirectoryNotFoundException();
            if (!MergedWallpaperDirectory.Exists)
                throw new DirectoryNotFoundException();
            if (!WorkingDirectoryNamed.Exists)
                throw new DirectoryNotFoundException();
            if (!WorkingDirectory.Exists)
                throw new DirectoryNotFoundException();
            if (!TrashDirectory.Exists)
                throw new DirectoryNotFoundException();
        }

        #endregion

        #region Methods

        internal static List<FileInfo> SortOutOrignals()
        {
            var movedFiles = new List<FileInfo>();
            //Go through all Orignal Files and see if they are not listed in the other folder. (O(n))

            var allOriginalFiles = OriginalWallpaperDirectory.GetFiles("*", SearchOption.AllDirectories);
            foreach (var currentFile in allOriginalFiles)
            {
                if (WallpaperFileHelper.IsImage(currentFile))
                {
                    if (currentFile.Name.Contains("#o"))
                    {
                        //if (currentFile.Name == "Paladins#o1.png")
                        //{
                        //    Console.WriteLine("Debug");
                        //}

                        //Check if there is a corresponding wallpaper file
                        var correspondingFileName = Path.GetFileNameWithoutExtension(currentFile.FullName).Replace("#o", "#");
                        var files = new[]
                        {
                            $"{correspondingFileName}.png", //Test for png
                            $"{correspondingFileName}.jpg", //Test for jpg
                        };
                        var foundFile = files.FirstOrDefault(file => System.IO.File.Exists(Path.Combine(WallpaperDirectory.FullName, currentFile.Directory.Name, file))); //Find the first path that exits

                        if (foundFile is null)
                        {
                            //Move file, it does not belong here
                            //currentFile.MoveTo(Path.Combine(TrashDirectory.FullName, currentFile.Name));
                            movedFiles.Add(currentFile);
                        }
                    }
                }
            }
            return movedFiles;
        }

        public static void FixAllIndexes()
        {
            Wallpapers.Sort(); //Make sure everything is sorted!

            var goodIndexes = 0;
            var correctedIndexes = 0;
            var correctedWallpapers = new List<Wallpaper>();

            var lastFranchise = Wallpapers[0].Franchise;
            var startIndex = 1;
            var nextIndex = startIndex; //Counting beginning from 1
            foreach (var currentWallpaper in Wallpapers)
            {
                if (lastFranchise != currentWallpaper.Franchise) //Franchise has changed
                {
                    lastFranchise = currentWallpaper.Franchise; //Set new Franchise to work in
                    nextIndex = startIndex; //Reset the counting index
                }

                //Check the index.
                if (nextIndex != currentWallpaper.Index)
                {
                    //Needs correction
                    currentWallpaper.SetIndexAndCorrectFiles(nextIndex);
                    correctedIndexes++;
                    correctedWallpapers.Add(currentWallpaper);
                }
                else
                {
                    //Everything good, do nothing
                    goodIndexes++;
                }
                nextIndex++;
            }

            Console.WriteLine($"Good: {goodIndexes}");
            Console.WriteLine($"Corrected: {correctedIndexes}");
            Console.WriteLine("\nList of corrected wallpapers:");
            foreach (var currentWallpaper in correctedWallpapers)
            {
                Console.WriteLine(currentWallpaper);
            }
        }

        public static void FullScan()
        {
            var allFiles = WallpaperDirectory.GetFiles("*", SearchOption.AllDirectories);
            foreach (var currentFile in allFiles)
            {
                if (WallpaperFileHelper.IsImage(currentFile))
                {
                    if (!currentFile.Name.Contains("#o"))
                    {
                        _ = new Wallpaper(currentFile, WallpaperCreationMode.Scan);
                    }
                }
            }
            Wallpapers.Sort();
        }

        public static void Reset()
        {
            Types.Clear();
            Franchises.Clear();
            Wallpapers.Clear();
        }

        public static Tuple<int, int> Merge()
        {
            int countDeleted = 0;
            int countCopied = 0;

            //Delete all old files
            foreach (var currentFile in MergedWallpaperDirectory.GetFiles())
            {
                if (WallpaperFileHelper.IsImage(currentFile))
                {
                    currentFile.Delete();
                    countDeleted++;
                }
            }

            //Get excluded types
            WallpaperType excludedType;
            if (WallpaperType.TryGet("X-MAS", out WallpaperType? type))
            {
                excludedType = type;
            }
            else
            {
                throw new ArgumentNullException("Franchise not found.", nameof(excludedType));
            }

            //Copy all wallpapers that are not excluded
            foreach (var currentWallpaper in Wallpapers)
            {
                if (currentWallpaper.Franchise.Type != excludedType)
                {
                    currentWallpaper.File.CopyTo(Path.Combine(MergedWallpaperDirectory.FullName, currentWallpaper.File.Name));
                    countCopied++;
                }
            }

            return new Tuple<int, int>(countDeleted, countCopied);
        }

        public static int CleanUpOriginalWallpaper()
        {
            var count = 0;
            var originalFileList = OriginalWallpaperDirectory.GetFiles("*", SearchOption.AllDirectories);

            foreach (var currentOriginalFile in originalFileList)
            {
                if (WallpaperFileHelper.IsImage(currentOriginalFile))
                {
                    bool fileFound = false;

                    foreach (var currentWallpaper in Wallpapers)
                    {
                        if (WallpaperFileHelper.ImageFilesMatch(currentWallpaper.File, currentOriginalFile))
                        {
                            fileFound = true;
                            break;
                        }
                    }

                    if (!fileFound)
                    {
                        currentOriginalFile.MoveTo(Path.Combine(TrashDirectory.FullName, currentOriginalFile.Name));
                        count++;
                    }
                }
            }
            return count;
        }

        public static List<Wallpaper> SortInNamedWorkingWallpapers()
        {
            var newWallpapers = new List<Wallpaper>();

            var typeDirectories = WorkingDirectoryNamed.GetDirectories();
            foreach (var currentTypeDirectory in typeDirectories)
            {
                var currentFiles = currentTypeDirectory.GetFiles("*", SearchOption.TopDirectoryOnly);
                foreach (var currentFile in currentFiles) //Go through the current type images
                {
                    if (WallpaperFileHelper.IsImage(currentFile)) //Make sure they are an image
                    {
                        WallpaperFileHelper.FixImageExtension(currentFile);
                        //not an original file
                        if (!currentFile.Name.Contains("#o"))
                        {
                            Wallpaper newWallpaper;

                            try
                            {
                                newWallpaper = new Wallpaper(currentFile, WallpaperCreationMode.New);
                            }
                            catch (WallpaperBuildExeption e)
                            {
                                if (e.UserAbortion)
                                {
                                    continue;
                                }
                                else
                                {
                                    throw e;
                                }
                            }

                            //Move it to the right location
                            newWallpaper.File.MoveTo(Path.Combine(WallpaperDirectory.FullName, newWallpaper.Franchise.Type.Name, $"{newWallpaper.Franchise.Name}#{newWallpaper.Index}{newWallpaper.File.Extension}"));
                            if (!(newWallpaper.OriginalFile is null))
                            {
                                newWallpaper.OriginalFile!.MoveTo(Path.Combine(OriginalWallpaperDirectory.FullName, newWallpaper.Franchise.Type.Name, $"{newWallpaper.Franchise.Name}#o{newWallpaper.Index}{newWallpaper.File.Extension}"));
                            }

                            newWallpapers.Add(newWallpaper);
                        }
                    }
                }

                //Now look for grouped images in franchise folders
                var currentTypeSubDirectories = currentTypeDirectory.GetDirectories();
                foreach (var currentTypeSubDiretory in currentTypeSubDirectories)
                {
                    var files = currentTypeSubDiretory.GetFiles("*", SearchOption.TopDirectoryOnly);

                    foreach (var currentFile in files)
                    {
                        if (WallpaperFileHelper.IsImage(currentFile))
                        {
                            WallpaperFileHelper.FixImageExtension(currentFile);

                            //Check if this file is not an original.
                            if (Regex.IsMatch(currentFile.Name, WallpaperManager.EditedFilesRegexString))
                            {
                                Wallpaper newWallpaper;

                                try
                                {
                                    newWallpaper = new Wallpaper(currentFile, WallpaperCreationMode.NewInSubFolder);
                                }
                                catch (WallpaperBuildExeption e)
                                {
                                    if (e.UserAbortion)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        throw e;
                                    }
                                }

                                //Move it to the right location
                                newWallpaper.File.MoveTo(Path.Combine(WallpaperDirectory.FullName, newWallpaper.Franchise.Type.Name, $"{newWallpaper.Franchise.Name}#{newWallpaper.Index}{newWallpaper.File.Extension}"));
                                if (!(newWallpaper.OriginalFile is null))
                                {
                                    newWallpaper.OriginalFile!.MoveTo(Path.Combine(OriginalWallpaperDirectory.FullName, newWallpaper.Franchise.Type.Name, $"{newWallpaper.Franchise.Name}#o{newWallpaper.Index}{newWallpaper.File.Extension}"));
                                }

                                newWallpapers.Add(newWallpaper);
                            }
                        }
                    }
                }
            }

            return newWallpapers;
        }

        #endregion
    }
}
