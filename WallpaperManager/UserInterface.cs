using System;
using System.Net.Http.Headers;

namespace WallpaperManager
{
    public static class UserInterface
    {
        #region Main Methods

        /// <summary>
        /// Starts the main sequence of the user interface and stays in an loop until the program is aborted.
        /// </summary>
        public static void Run()
        {
            Scan(true);

            while (true)
            {
                MainMenu();
            }
        }

        /// <summary>
        /// Displays the main menu with all features and prompts the user to choose one.
        /// </summary>
        public static void MainMenu()
        {
            ClearAndPrintHeadline();
            Console.WriteLine("1 - Reload all images");
            Console.WriteLine("2 - Show wallpaper stats");
            Console.WriteLine("3 - Merge wallpapers");
            Console.WriteLine("4 - Sort in new and named wallpapers");
            Console.WriteLine("5 - Sort out original files without a wallpaper file.");
            Console.WriteLine("6 - Fix all wallpaper indexes");
            Console.WriteLine("7 - Exit");

            switch (GetUserInput(7))
            {
                case 1:
                    Scan(false);
                    break;
                case 2:
                    PrintWallpaperStats();
                    break;
                case 3:
                    Merge();
                    break;
                case 4:
                    SortInNamedWorkingWallpapers();
                    break;
                case 5:
                    SortOutOrignals();
                    break;
                case 6:
                    FixAllIndexes();
                    break;
                case 7:
                    Environment.Exit(0);
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Main Menu Methods

        /// <summary>
        /// Scans or rescans all wallpapers and their originals. 
        /// </summary>
        /// <param name="firstScan">Indicates whether it is the first scan or not. This determines if exiting data is deleted before scanning or not.</param>
        public static void Scan(bool firstScan = false)
        {
            ClearAndPrintHeadline();
            if (firstScan)
            {
                Console.WriteLine("Initiation Process...\n");
            }
            else
            {
                Console.WriteLine("Deleting existing information...");
                WallpaperManager.Reset();
                Console.WriteLine("Rescanning...\n");
            }
            Console.WriteLine($"Looking for wallpapers in {WallpaperManager.WallpaperDirectory.FullName}");
            Console.WriteLine($"Looking for original versions in {WallpaperManager.OriginalWallpaperDirectory.FullName}");
            WallpaperManager.FullScan();
            Console.WriteLine($"Found {WallpaperManager.Wallpapers.Count} wallpapers and {WallpaperManager.Wallpapers.Count - Wallpaper.TotalCountWithoutOriginal} originals.");

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Prints some interesting statistics about all known wallpapers
        /// </summary>
        public static void PrintWallpaperStats()
        {
            ClearAndPrintHeadline();

            var Wallpapers = WallpaperManager.Wallpapers;
            var Franchises = WallpaperManager.Franchises;
            var Types = WallpaperManager.Types;

            Console.WriteLine($"Total count of Wallpapers: {Wallpapers.Count}");
            Console.WriteLine($"Total count of Wallpapers without an original file: {Wallpaper.TotalCountWithoutOriginal} ({Math.Round(Wallpaper.TotalCountWithoutOriginal / (double)Wallpapers.Count * 100, 0):00}%)");

            PressAnyKeyToContinue("Press any key to show more info");
            Console.WriteLine($"\nTotal count of Types: {Types.Count}");
            foreach (var currentType in Types)
            {
                //TODO: Improve this count lookup algorithm. (mxn -> n)
                Console.WriteLine($"Name: {currentType.Name,-13} | Count: {Wallpapers.FindAll(w => w.Franchise.Type == currentType).Count}");
            }

            PressAnyKeyToContinue("Press any key to show more info");
            Console.WriteLine($"\nTotal count of Franchises: {Franchises.Count}");
            var previousType = "";
            foreach (var currentFranchise in Franchises)
            {
                var franchiseName = currentFranchise.Name.Length >= 35 ? currentFranchise.Name.Substring(0, 32) + "..." : currentFranchise.Name.PadRight(35);
                Console.WriteLine($"Name: {franchiseName} | {(currentFranchise.Type.Name != previousType ? "Type : " + currentFranchise.Type.Name : "")}");

                previousType = currentFranchise.Type.Name;
            }

            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Copies all wallpapers into one folder. This is useful when another program (like windows backgrounds) need one folder.
        /// </summary>
        public static void Merge()
        {
            ClearAndPrintHeadline();
            var info = WallpaperManager.Merge();
            Console.WriteLine($"Deleted {info.Item1} Images");
            Console.WriteLine($"Copied {info.Item2} Images");
            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Starts the sorting in process for new wallpapers located in the working directory.
        /// </summary>
        private static void SortInNamedWorkingWallpapers()
        {
            ClearAndPrintHeadline();
            var newWallpapers = WallpaperManager.SortInNamedWorkingWallpapers();
            Console.WriteLine($"\nAdded and moved {newWallpapers.Count} wallpaper.");
            foreach (var currentWallpaper in newWallpapers)
            {
                Console.WriteLine(currentWallpaper.ToString());
            }
            PressAnyKeyToContinue();

        }

        /// <summary>
        /// Finds and moves original wallpapers that have no edited version and therefor have no purpose.
        /// </summary>
        private static void SortOutOrignals()
        {
            ClearAndPrintHeadline();
            var movedFiles = WallpaperManager.SortOutOrignals();
            foreach (var currentFile in movedFiles)
            {
                Console.WriteLine($"{currentFile.Directory.Name}/{currentFile.Name} was moved.");
            }
            PressAnyKeyToContinue();
        }

        /// <summary>
        /// Goes through all index numbers, checks if they are in order without a number missing and fixes them when necessary.
        /// </summary>
        private static void FixAllIndexes()
        {
            ClearAndPrintHeadline();
            WallpaperManager.FixAllIndexes();
            PressAnyKeyToContinue();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Clears the console and prints the name of the program along with its current version number as headline.
        /// </summary>
        public static void ClearAndPrintHeadline()
        {
            Console.Clear();
            var welcomeLine = $"adiac's Wallpaper Manager {Program.Version}";
            Console.WriteLine(welcomeLine);
            Console.WriteLine(new String('=', welcomeLine.Length));
            Console.WriteLine();
        }

        /// <summary>
        /// Print a message and waits until the user presses any key.
        /// </summary>
        /// <param name="prompt"></param>
        public static void PressAnyKeyToContinue(string prompt = "Press any key to continue")
        {
            Console.Write("\n" + prompt);
            Console.ReadKey();

            if (prompt != "Press any key to continue")
            {
                Console.CursorLeft = 0;
                Console.WriteLine(new String(' ', prompt.Length + 1));
                Console.CursorLeft = 0;
                Console.CursorTop -= 2;
            }
            else
            {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Gets a user input to a question that can be answered with yes or no. Wrong inputs will be ignored.
        /// </summary>
        /// <returns></returns>
        public static bool GetUserInput()
        {
            Console.Write("\nPlease Type y/n: ");
            while (true)
            {
                var inputString = Console.ReadKey().KeyChar;

                if (inputString == 'y' || inputString == 'Y')
                {
                    return true;
                }
                else if (inputString == 'n' || inputString == 'N')
                {
                    return false;
                }
                Console.CursorLeft--;
                Console.Write(" ");
                Console.CursorLeft--;
            }
        }

        /// <summary>
        /// Gets a user input to a question that can be answered with a number from 1 to 9. Wrong inputs will be ignored.
        /// </summary>
        /// <param name="amountOptions">Defines the range of options the user can enter.</param>
        /// <returns></returns>
        public static int GetUserInput(int amountOptions)
        {
            bool isNumeric = false;
            Console.Write("\nPlease enter a number from above: ");

            while (true)
            {
                var inputThing = Console.ReadKey().KeyChar;
                isNumeric = int.TryParse(inputThing.ToString(), out int choosenOption);
                if (isNumeric && choosenOption <= amountOptions && choosenOption > 0)
                {
                    return choosenOption;
                }
                Console.CursorLeft--;
                Console.Write(" ");
                Console.CursorLeft--;
            }
        }

        #endregion
    }
}
