using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class ImageUploadValidator
    {
        public static bool IsWebFriendlyImage(HttpPostedFileBase file)
        {
            if (file == null)
                return false;

            if (file.ContentLength > 3 * 1024 * 1024 || file.ContentLength < 1024)
                return false;

            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                           ImageFormat.Png.Equals(img.RawFormat) ||
                           ImageFormat.Gif.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }

        }
    }

    public class FileUploadValidator
    {
        public static bool IsFileAcceptableFormat(HttpPostedFileBase file)
        {
            if (file == null)
                return false;

            if (file.ContentLength > 3 * 1024 * 1024 || file.ContentLength < 1)
                return false;

            var allowedExtensions = new[] { ".pdf", ".zip", ".doc", ".docx", ".txt", ".rtf" };
            var checkextension = Path.GetExtension(file.FileName).ToLower();

            if (allowedExtensions.Contains(checkextension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}