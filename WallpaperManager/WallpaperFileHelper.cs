using System;
using System.IO;
using System.Linq;

namespace WallpaperManager
{
    public static class WallpaperFileHelper
    {
        public static void FixImageExtension(FileInfo file)
        {
            if (file.Extension == ".jpeg")
            {
                RenameFile(file, $"{Path.GetFileNameWithoutExtension(file.Name)}.jpg", false);
                Console.WriteLine($"An unnormalized file extension has been found and corrected. {file.FullName}");
            }
        }

        public static bool IsImage(FileInfo file)
        {
            var fileExtensions = new string[3];
            fileExtensions[0] = ".png";
            fileExtensions[1] = ".jpg";
            fileExtensions[2] = ".jpeg";

            var foundExtension = fileExtensions.FirstOrDefault(ext => ext == file.Extension);

            if (foundExtension is null)
            {
                return false;
            }

            FixImageExtension(file);
            return true;
        }

        public static bool ImageFilesMatch(FileInfo fileA, FileInfo fileB)
        {
            var fileNameA = Path.GetFileNameWithoutExtension(fileA.FullName).Replace("#o", "#");
            var fileNameB = Path.GetFileNameWithoutExtension(fileB.FullName).Replace("#o", "#");
            return fileNameA == fileNameB;
        }

        public static void RenameFile(FileInfo file, string newName, bool keepExtension = true)
        {
            if (keepExtension)
            {
                file.MoveTo(Path.Combine(file.Directory.FullName, $"{newName}{file.Extension}"));
            }
            else
            {
                file.MoveTo(Path.Combine(file.Directory.FullName, newName));
            }
        }
    }
}
