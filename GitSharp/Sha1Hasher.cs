using System.Security.Cryptography;

namespace GitSharp
{
    public class Sha1Hasher
    {
        public byte[] CreateHash(byte[] inputBytes)
        {
            using (var algorithm = SHA1.Create())
            {
                var hash = algorithm.ComputeHash(inputBytes);
                return hash;
            }
        }
    }
}