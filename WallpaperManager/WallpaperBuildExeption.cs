using System;
using System.Collections.Generic;
using System.Text;

namespace WallpaperManager
{
    class WallpaperBuildExeption : Exception
    {
        public bool UserAbortion { get; } = false;
        public object[]? ObjectsToDelte { get; }

        public WallpaperBuildExeption() : base() { }
        public WallpaperBuildExeption(string? message) : base(message) { }
        public WallpaperBuildExeption(string? message, bool userAbortion) : base(message)
        {
            UserAbortion = userAbortion;
        }

        public WallpaperBuildExeption(string? message, bool userAbortion, params object[] objectsToDelte) : this(message, userAbortion)
        {
            ObjectsToDelte = objectsToDelte;
        }


    }
}
