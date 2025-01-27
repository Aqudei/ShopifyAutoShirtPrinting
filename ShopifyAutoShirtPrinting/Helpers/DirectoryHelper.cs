using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyEasyShirtPrinting.Helpers
{
    public class DirectoryHelper
    {
        public static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return GetDirectorySize(di);
        }

        public static long GetDirectorySize(DirectoryInfo directory)
        {
            long size = 0;

            // Add file sizes.
            FileInfo[] fis = directory.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }

            // Add subdirectory sizes.
            DirectoryInfo[] dis = directory.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }

            return size;
        }


        // Replace invalid filename characters with underscores
        public static string SanitizeFilename(string filename, char replacementChar = '_')
        {
            // Get the array of invalid characters for file names in the current OS
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Replace each invalid character with the replacement character
            foreach (char invalidChar in invalidChars)
            {
                filename = filename.Replace(invalidChar, replacementChar);
            }

            // Optional: Trim whitespace from the ends of the filename
            filename = filename.Trim();

            // Optional: Restrict the length of the filename to a certain maximum length (e.g., 255 characters)
            int maxLength = 255;
            if (filename.Length > maxLength)
            {
                filename = filename.Substring(0, maxLength);
            }

            return filename;
        }

        public static string FormatSize(long bytes)
        {
            const int scale = 1024;

            double formattedNumber;
            string suffix;

            if (bytes >= Math.Pow(scale, 3)) // Gigabytes
            {
                formattedNumber = (double)bytes / Math.Pow(scale, 3);
                suffix = "GB";
            }
            else if (bytes >= Math.Pow(scale, 2)) // Megabytes
            {
                formattedNumber = (double)bytes / Math.Pow(scale, 2);
                suffix = "MB";
            }
            else if (bytes >= scale) // Kilobytes
            {
                formattedNumber = (double)bytes / scale;
                suffix = "KB";
            }
            else // Bytes
            {
                formattedNumber = bytes;
                suffix = "bytes";
            }

            return string.Format("{0:0.##} {1}", formattedNumber, suffix);
        }
    }
}
