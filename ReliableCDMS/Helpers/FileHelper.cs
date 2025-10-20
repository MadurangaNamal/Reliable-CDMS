using System;
using System.IO;
using System.Linq;

namespace ReliableCDMS.Helpers
{
    public static class FileHelper
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Sanitize filename to prevent path traversal attacks
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            fileName = Path.GetFileName(fileName);

            // Replace invalid characters
            foreach (char charater in InvalidFileNameChars)
            {
                fileName.Replace(charater, '_');
            }

            // Remove dangerous characters (any remaining)
            fileName = fileName.Replace("..", "_");
            fileName = fileName.Replace("/", "_");
            fileName = fileName.Replace("\\", "_");

            // Remove dots and spaces
            fileName = fileName.Trim('.', ' ');

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "unnamed";

            const int maxLength = 200;

            if (fileName.Length > maxLength)
            {
                string typeExtension = Path.GetExtension(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                name = name.Substring(0, maxLength - typeExtension.Length);
                fileName = name + typeExtension;
            }

            return fileName;
        }

        /// <summary>
        /// Validate file extension against allowed types
        /// </summary>
        public static bool IsAllowedFileType(string fileName, string[] allowedExtensions = null)
        {
            if (allowedExtensions == null || allowedExtensions.Length == 0)
            {
                // If no restrictions, allow all
                return true;
            }

            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            return allowedExtensions.Any(ext => ext.ToLowerInvariant() == extension);
        }

        /// <summary>
        /// Get safe file path for upload
        /// </summary>
        public static string GetSafeUploadPath(string fileName, string uploadFolder)
        {
            // Sanitize filename
            string safeFileName = SanitizeFileName(fileName);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + safeFileName;

            // Combine with upload folder
            string fullPath = Path.Combine(uploadFolder, uniqueFileName);

            // Validate the path - prevents directory traversal
            string normalizedUploadFolder = Path.GetFullPath(uploadFolder);
            string normalizedFullPath = Path.GetFullPath(fullPath);

            if (!normalizedFullPath.StartsWith(normalizedUploadFolder, StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException("Invalid file path detected");
            }

            return fullPath;
        }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message)
            : base(message) { }
    }
}