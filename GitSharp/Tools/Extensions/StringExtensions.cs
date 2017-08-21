using System;
using System.Linq;
using System.Text;

namespace GitSharp.Tools.Extensions
{
    public static class StringExtensions
    {
        public static string GetRelativePath(this string absolutePath, string absoluteRootPath)
        {
            if (string.IsNullOrEmpty(absolutePath)) throw new ArgumentNullException(nameof(absolutePath));
            if (string.IsNullOrEmpty(absoluteRootPath)) throw new ArgumentNullException(nameof(absoluteRootPath));
            if (absolutePath.Equals(absoluteRootPath, StringComparison.InvariantCultureIgnoreCase)) return string.Empty;
            return absolutePath.Replace($"{absoluteRootPath}\\", "");
        }

        public static byte[] GetUtf8Bytes(this string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));
            return Encoding.UTF8.GetBytes(input);
        }

        public static string GetHash(this string input, IHasher hasher)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashedBytes = hasher.CreateHash(bytes);
            var hash = string.Join(string.Empty, hashedBytes.Select(b => b.ToString("X2"))).ToLower();
            return hash;
        }
    }
}