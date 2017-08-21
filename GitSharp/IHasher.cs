namespace GitSharp
{
    public interface IHasher
    {
        byte[] CreateHash(byte[] inputBytes);
    }
}