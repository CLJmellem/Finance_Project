using Auth.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Encryption Service
/// </summary>
public class EncryptionService(IConfiguration configuration) : IEncryptionService
{
    /// <summary>The key</summary>
    public byte[] _key = Convert.FromHexString(configuration["Encryption:Key"]);

    /// <summary>Encrypts the specified text to encrypt.</summary>
    /// <param name="textToEncrypt">The text to encrypt.</param>
    /// <returns></returns>
    public string Encrypt(string textToEncrypt)
    {
        byte[] encrypted;
        Aes aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(textToEncrypt);
                }
                encrypted = msEncrypt.ToArray();
            }
        }

        return Convert.ToBase64String(encrypted);
    }

    /// <summary>Decrypts the specified text to decrypt.</summary>
    /// <param name="textToDecrypt">The text to decrypt.</param>
    /// <returns></returns>
    public string Decrypt(string textToDecrypt)
    {
        string decrypted;
        Aes aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(textToDecrypt)))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    decrypted = srDecrypt.ReadToEnd();
                }
            }
        }

        return decrypted;
    }
}