using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using WallpaperManager.WallpaperClass;

namespace WallpaperManager
{
    partial class Wallpaper : IComparable
    {
        #region Fields and Properties

        public FileInfo File { get; }
        public FileInfo? OriginalFile { get; }
        public WallpaperFranchise Franchise { get; }
        public int Index { get; private set; }

        public static int TotalCountWithoutOriginal
        {
            get
            {
                int count = 0;
                foreach (var currentWallaper in WallpaperManager.Wallpapers)
                {
                    if (currentWallaper.OriginalFile is null)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        #endregion

        #region C'tor

        //New Powerfull C'tor
        public Wallpaper(FileInfo file, WallpaperCreationMode mode)
        {
            WallpaperDataContainer container;

            if (!file.Exists)
                throw new FileNotFoundException("File was not found.");
            if (!WallpaperFileHelper.IsImage(file))
                throw new ArgumentException("Is not an image file.", nameof(file));

            //All parameters that have to be set.
            string typeName;
            string franchiseName;
            bool ignoreMissing;
            int? index;
            DirectoryInfo originalLookUpDirectory;
            string[] possibleOriginalFileNames;

            //Helpers

            //Setting the parameters.
            switch (mode)
            {
                case WallpaperCreationMode.Scan: // --------------------------------------------------------------------------------------------
                    if (file.Name.Contains("#o"))
                        throw new ArgumentException("Is an original file.", nameof(file));
                    if (!Regex.IsMatch(file.Name, "\\S+#\\d+"))
                        throw new FileLoadException("Image file was not in the correct normalized format.", file.Name);

                    typeName = file.Directory.Name;
                    var shortFileName1 = Path.GetFileNameWithoutExtension(file.Name);
                    var splitShortFileName1 = shortFileName1.Split('#'); //The '#' separates franchise and index.
                    franchiseName = splitShortFileName1[0];
                    ignoreMissing = true;
                    index = Convert.ToInt32(splitShortFileName1[1]);
                    originalLookUpDirectory = new DirectoryInfo(Path.Combine(WallpaperManager.OriginalWallpaperDirectory.FullName, typeName));
                    possibleOriginalFileNames = new[]
                    {
                        $"{franchiseName}#o{index}.png", //Test for png
                        $"{franchiseName}#o{index}.jpg", //Test for jpg
                    };
                    break;
                case WallpaperCreationMode.New: // --------------------------------------------------------------------------------------------
                    typeName = file.Directory.Name;
                    var shortFileName2 = Path.GetFileNameWithoutExtension(file.Name);
                    var splitShortFileName2 = shortFileName2.Split('#'); //The '#' separates franchise and index.
                    if (splitShortFileName2.Length > 2) //Check if something has gone wrong while splitting
                        throw new ArgumentException("Is not in the correct format.", nameof(file));
                    franchiseName = splitShortFileName2[0];
                    ignoreMissing = false;
                    index = null; //Set a new index, the current one is just temporary
                    originalLookUpDirectory = file.Directory;

                    //Determine if there is a temp index given. In a not normalized format, an temp index is not mandatory.
                    string fileNameIndex;
                    if (splitShortFileName2.Length == 2)
                    {
                        if (Int32.TryParse(splitShortFileName2[1], out var _))
                        {
                            fileNameIndex = splitShortFileName2[1];
                        }
                        else
                        {
                            throw new ArgumentException("Could not parse index number.", nameof(file));
                        }
                    }
                    else //splitShortFileName2.Length == 1
                    {
                        fileNameIndex = String.Empty;
                    }

                    possibleOriginalFileNames = new[]
                    {
                        $"{franchiseName}#o{fileNameIndex}.png", //Test for png
                        $"{franchiseName}#o{fileNameIndex}.jpg", //Test for jpg
                    };
                    break;
                case WallpaperCreationMode.NewInSubFolder: // --------------------------------------------------------------------------------------------
                    typeName = file.Directory.Parent.Name;
                    franchiseName = file.Directory.Name;
                    ignoreMissing = false;
                    index = null;
                    originalLookUpDirectory = file.Directory;
                    var matchLength = Regex.Match(file.Name, WallpaperManager.EditedFilesRegexString).Value.Length;
                    var originalFileNameWithoutExtension = file.Name.Substring(0, file.Name.Length - matchLength);
                    possibleOriginalFileNames = new[]
                    {
                        $"{originalFileNameWithoutExtension}.png", //Test for png
                        $"{originalFileNameWithoutExtension}.jpg", //Test for jpg
                    };
                    break;
                default:
                    throw new WallpaperBuildExeption($"Unknown {nameof(WallpaperCreationMode)}.");
            }

            try
            {
                container = ContructorHelper(file, typeName, franchiseName, ignoreMissing, index, originalLookUpDirectory, possibleOriginalFileNames);
            }
            catch (WallpaperBuildExeption e)
            {
                if (e.UserAbortion && !(e.ObjectsToDelte is null)) //Handle objects that need to be removed, when the process was aborted.
                {
                    foreach (var currentObject in e.ObjectsToDelte)
                    {
                        if (currentObject is WallpaperType)
                        {
                            WallpaperManager.Types.Remove((WallpaperType)currentObject);
                        }
                        else
                        {
                            throw new ArgumentException($"Object {nameof(currentObject)} is an unexpected type.");
                        }
                    }
                }
                throw e;
            }

            File = file;
            OriginalFile = container.OriginalFile;
            Franchise = container.Franchise;
            Index = container.Index;
            WallpaperManager.Wallpapers.Add(this); //Add this instance to the list of all wallpapers.
        }

        #endregion

        #region C'Tor Helper

        private WallpaperDataContainer ContructorHelper(FileInfo file, string typeName, string franchiseName, bool ignoreMissing, int? index, DirectoryInfo originalLookUpDirectory, string[] possibleOriginalFileNames)
        {
            if (!originalLookUpDirectory.Exists)
                throw new DirectoryNotFoundException();

            //Find Type
            GetOrCreateType(typeName, ignoreMissing, out var finalType);

            //Find Franchise
            GetOrCreateFranchise(franchiseName, finalType, ignoreMissing, out var finalFranchise);

            //Find Index
            int finalIndex;
            if (index is null)
            {
                finalIndex = finalFranchise.GetNextIndex();
            }
            else
            {
                finalIndex = (int)index;
            }

            //Get the original file
            if (!FindOriginal(possibleOriginalFileNames, finalType, originalLookUpDirectory, out var finalOriginalFile) && !ignoreMissing)
            {
                Console.Write($"Original file for {file.Name} not found. It seems there is none. Is this is intentional?");
                if (!UserInterface.GetUserInput())
                {
                    Console.WriteLine();
                    throw new WallpaperBuildExeption($"Original file for {File.Name} not found.", true);
                }
                Console.WriteLine();
            }

            return new WallpaperDataContainer(finalOriginalFile, finalFranchise, finalIndex);
        }

        /// <summary>
        /// Gets the type with the given name. If it does not exit the user is asked if it should be created.
        /// </summary>
        /// <param name="typeName">The name of the franchise type to get or create</param>
        /// <param name="wallpaperFranchiseType">The found or created franchise type</param>
        /// <returns>If a new franchise type was created.</returns>
        private void GetOrCreateType(string typeName, bool ignoreMissing, out WallpaperType wallpaperFranchiseType)
        {
            if (ignoreMissing)
            {
                wallpaperFranchiseType = WallpaperType.GetOrCreate(typeName);
            }
            else
            {
                if (WallpaperType.TryGet(typeName, out WallpaperType? foundType)) //Check if type is unknown
                {
                    wallpaperFranchiseType = foundType;
                }
                else
                {
                    Console.Write($"The wallpaper type with the name '{typeName}' does not exist. Do you want to create it?");
                    if (UserInterface.GetUserInput())
                    {
                        wallpaperFranchiseType = new WallpaperType(typeName);
                    }
                    else
                    {
                        Console.WriteLine();
                        throw new WallpaperBuildExeption("Franchise Type not found and not created.", true);
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Gets the franchise with the given name. If it does not exit the user is asked if it should be created.
        /// </summary>
        /// <param name="franchiseName">The name of the franchise to get or create.</param>
        /// <param name="wallpaperFranchiseType">The type of the franchise.</param>
        /// <param name="wallpaperFranchise">The found or created franchise.</param>
        /// <returns>If new franchise was created.</returns>
        private void GetOrCreateFranchise(string franchiseName, WallpaperType wallpaperFranchiseType, bool ignoreMissing, out WallpaperFranchise wallpaperFranchise)
        {
            if (ignoreMissing)
            {
                wallpaperFranchise = WallpaperFranchise.GetOrCreate(franchiseName, wallpaperFranchiseType);
            }
            else
            {
                if (WallpaperFranchise.TryGet(franchiseName, out WallpaperFranchise? foundFranchise)) //Check if franchise already exists
                {
                    if (foundFranchise.Type == wallpaperFranchiseType)
                    {
                        //All okay
                        wallpaperFranchise = foundFranchise;
                    }
                    else
                    {
                        throw new WallpaperBuildExeption($"Given and found {nameof(WallpaperType)} do not match");
                    }
                }
                else
                {
                    Console.Write($"The wallpaper franchise with the name '{franchiseName}' does not exist. It would belong to the type '{wallpaperFranchiseType.Name}'.\nDo you want to create it?");
                    if (UserInterface.GetUserInput())
                    {
                        wallpaperFranchise = new WallpaperFranchise(franchiseName, wallpaperFranchiseType);
                    }
                    else
                    {
                        Console.WriteLine();
                        throw new WallpaperBuildExeption("Franchise not found and not created.", true, wallpaperFranchiseType);
                    }
                    Console.WriteLine();
                }
            }
        }

        private bool FindOriginal(string[] originalFileNames, WallpaperType type, DirectoryInfo lookUpDirectory, [NotNullWhen(true)] out FileInfo? originalFile)
        {
            //Find the first path that exits
            var foundFile = originalFileNames.FirstOrDefault(testFile => System.IO.File.Exists(Path.Combine(lookUpDirectory.FullName, testFile)));
            if (foundFile is null)
            {
                originalFile = null;
                return false;
            }
            originalFile = new FileInfo(Path.Combine(lookUpDirectory.FullName, foundFile));
            return true;
        }

        #endregion

        #region Public Methods

        public void SetIndexAndCorrectFiles(int newIndex)
        {
            Index = newIndex;
            WallpaperFileHelper.RenameFile(File, $"{Franchise.Name}#{Index}");
            if (!(OriginalFile is null))
                WallpaperFileHelper.RenameFile(OriginalFile, $"{Franchise.Name}#o{Index}");
        }

        #endregion

        #region Overloaded Methods

        public override string ToString()
        {
            return $"{Franchise.Type.Name}/{Franchise.Name}#{Index}";
        }

        public int CompareTo(object? obj)
        {
            if (obj is null) return 1;

            Wallpaper? otherWallpaper = obj as Wallpaper;
            if (!(otherWallpaper is null))
            {
                var typeCompareResult = Franchise.Type.Name.CompareTo(otherWallpaper.Franchise.Type.Name);
                if (typeCompareResult != 0)
                {
                    return typeCompareResult;
                }
                var franchiseCompareResult = Franchise.Name.CompareTo(otherWallpaper.Franchise.Name);
                if (franchiseCompareResult != 0)
                {
                    return franchiseCompareResult;
                }
                return Index.CompareTo(otherWallpaper.Index);
            }
            else
                throw new ArgumentException("Object is not a Wallpaper");
        }

        #endregion
    }
}
