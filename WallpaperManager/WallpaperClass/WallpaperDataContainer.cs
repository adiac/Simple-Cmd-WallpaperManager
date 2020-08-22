using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WallpaperManager.WallpaperClass
{
    class WallpaperDataContainer
    {
        public FileInfo? OriginalFile { get; }
        public WallpaperFranchise Franchise { get; }
        public int Index { get; }

        public WallpaperDataContainer(FileInfo? originalFile, WallpaperFranchise franchise, int index)
        {
            OriginalFile = originalFile;
            Franchise = franchise;
            Index = index;
        }
    }
}
