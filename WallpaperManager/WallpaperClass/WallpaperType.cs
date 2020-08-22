using System;
using System.Diagnostics.CodeAnalysis;

namespace WallpaperManager
{
    class WallpaperType
    {
        public string Name { get; }

        public WallpaperType(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Returns a instance with the given name or creates a new one if there is none.
        /// </summary>
        /// <param name="franchiseName"></param>
        /// <param name="franchiseType"></param>
        /// <returns></returns>
        public static WallpaperType GetOrCreate(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            foreach (var currentType in WallpaperManager.Types)
            {
                if (currentType.Name == typeName)
                {
                    return currentType;
                }
            }

            var newType = new WallpaperType(typeName);
            WallpaperManager.Types.Add(newType);
            return newType;
        }

        public static bool TryGet(string typeName, [NotNullWhen(true)] out WallpaperType? type)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            foreach (var currentType in WallpaperManager.Types)
            {
                if (currentType.Name == typeName)
                {
                    type = currentType;
                    return true;
                }
            }

            type = null;
            return false; ;
        }
    }
}
