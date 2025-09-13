using System.Security.Cryptography;

namespace PaperTrail.Core.Services;

public class HashService
{
    public string ComputeHash(byte[] data)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(data));
    }

    public string ComputeHash(Stream stream)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }
}
