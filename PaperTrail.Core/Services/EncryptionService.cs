using System.Security.Cryptography;
using System.Text;

namespace PaperTrail.Core.Services;

public class EncryptionService
{
    public string Encrypt(string plaintext, string password)
    {
        using var aes = Aes.Create();
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        aes.Key = key.GetBytes(32);
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var result = new byte[16 + aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(salt, 0, result, 0, 16);
        Buffer.BlockCopy(aes.IV, 0, result, 16, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, result, 16 + aes.IV.Length, cipher.Length);
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText, string password)
    {
        var data = Convert.FromBase64String(cipherText);
        var salt = data.AsSpan(0,16).ToArray();
        var iv = data.AsSpan(16,16).ToArray();
        var cipher = data.AsSpan(32).ToArray();
        using var aes = Aes.Create();
        var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        aes.Key = key.GetBytes(32);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher,0,cipher.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
