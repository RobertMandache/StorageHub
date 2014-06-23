using Google.Apis.Drive.v2.Data;
using Microsoft.Win32;
using StorageHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageHub.Utility
{
    public class Util
    {

        public static string GetBytesReadable(long? nullableSize)
        {
            long size;
            if (nullableSize == null) return "";
            else size = (long)nullableSize;
            string sign = (size < 0 ? "-" : "");
            double readable = (size < 0 ? -size : size);
            string suffix;
            if (size >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (double)(size >> 50);
            }
            else if (size >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (double)(size >> 40);
            }
            else if (size >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (double)(size >> 30);
            }
            else if (size >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (double)(size >> 20);
            }
            else if (size >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (double)(size >> 10);
            }
            else if (size >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = (double)size;
            }
            else
            {
                return size.ToString(sign + "0 B"); // Byte
            }
            readable /= 1024;

            return sign + readable.ToString("0.# ") + suffix;
        }

        public static void ClearDropboxCache(string userId)
        {
            string cacheKey = userId + StorageService.ServiceTypes.Dropbox;
            HttpContext.Current.Cache.Remove(cacheKey);
            HttpContext.Current.Cache.Remove(cacheKey + "dict");           
        }

        public static void ClearDriveCache(string userId)
        {
            string cacheKey = userId + StorageService.ServiceTypes.GoogleDrive;
            HttpContext.Current.Cache.Remove(cacheKey);            
        }
    }
}