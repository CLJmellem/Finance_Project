namespace Auth.Application.Interfaces;

/// <summary>
/// Encryption Service Interface
/// </summary>
public interface IEncryptionService
{
    /// <summary>Encrypts the specified text to encrypt.</summary>
    /// <param name="textToEncrypt">The text to encrypt.</param>
    /// <returns></returns>
    string Encrypt(string textToEncrypt);

    /// <summary>Decrypts the specified text to decrypt.</summary>
    /// <param name="textToDecrypt">The text to decrypt.</param>
    /// <returns></returns>
    string Decrypt(string textToDecrypt);
}