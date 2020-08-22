using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WallpaperManager
{
    class WallpaperFranchise
    {
        public string Name { get;}
        public WallpaperType Type { get; }

        public WallpaperFranchise(string name, WallpaperType type)
        {
            Name = name;
            Type = type;
        }

        public static WallpaperFranchise GetOrCreate(string franchiseName, WallpaperType type)
        {
            if (string.IsNullOrWhiteSpace(franchiseName))
                throw new ArgumentException(nameof(franchiseName));

            foreach (var currentFranchise in WallpaperManager.Franchises)
            {
                if (currentFranchise.Name == franchiseName)
                {
                    return currentFranchise;
                }
            }

            var newFranchise = new WallpaperFranchise(franchiseName, type);
            WallpaperManager.Franchises.Add(newFranchise);
            return newFranchise;
        }

        public static bool TryGet(string franchiseName, [NotNullWhen(true)] out WallpaperFranchise? franchise)
        {
            if (string.IsNullOrWhiteSpace(franchiseName))
                throw new ArgumentException(nameof(franchiseName));

            foreach (var currentFranchise in WallpaperManager.Franchises)
            {
                if (currentFranchise.Name == franchiseName)
                {
                    franchise = currentFranchise;
                    return true;
                }
            }

            franchise = null;
            return false;
        }

        public static List<Wallpaper> GetAllWallpapers(WallpaperFranchise franchise)
        {
            return WallpaperManager.Wallpapers.FindAll(w => w.Franchise == franchise);
        }

        public List<Wallpaper> GetAllWallpapers()
        {
            return WallpaperManager.Wallpapers.FindAll(w => w.Franchise == this);
        }
    }
}
