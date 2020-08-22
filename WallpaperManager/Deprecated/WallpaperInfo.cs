using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WallpaperManager
{
    /// <summary>
    /// Deprecated
    /// </summary>
    public struct WallpaperInfo
    {
        public string TypeName { get; }
        public string FranchiseName { get; }
        public int Index { get; }

        /// <summary>
        /// Deprecated! Creates a new instance of a WallpaperInfo struct.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="isNormalized"></param>
        public WallpaperInfo(FileInfo file, bool isNormalized = true)
        {
            if (!WallpaperFileHelper.IsImage(file))
                throw new ArgumentException("Is not an image file.", nameof(file));
            if (file.Name.Contains("#o"))
                throw new ArgumentException("Is an original file.", nameof(file));

            if (isNormalized)
            {
                TypeName = file.Directory.Name;

                var shortFileName = Path.GetFileNameWithoutExtension(file.Name);
                var splitShortFileName = shortFileName.Split('#'); //The '#' separates franchise and index.
                FranchiseName = splitShortFileName[0];
                Index = Convert.ToInt32(splitShortFileName[1]);
            }
            else
            {
                TypeName = file.Directory.Name;

                var shortFileName = Path.GetFileNameWithoutExtension(file.Name);
                var splitShortFileName = shortFileName.Split('#'); //The '#' separates franchise and index.
                if (splitShortFileName.Length > 2) //Check if something has gone wrong while splitting
                    throw new ArgumentException("Is not in the correct format.", nameof(file));
                FranchiseName = splitShortFileName[0];

                if (splitShortFileName.Length == 2 && splitShortFileName[1] != String.Empty) //Determine if there is a index given. In a not normalized format, an index is not mandatory.
                {
                    if (Int32.TryParse(splitShortFileName[1], out int result))
                    {
                        Index = result;
                    }
                    else
                    {
                        throw new ArgumentException("Could not parse index number.", nameof(file));
                    }
                }
                else //-1 means that there is no meaningful index.
                {
                    Index = -1;
                }
            }
        }
    }
}
