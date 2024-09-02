using System;
using System.Security.Cryptography;
using System.Text;

public static class EncryptHelper
{
    // 获得AES加密后的值
    public static string Encrypt(string data, string key)
    {
        var payload = data;
        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.Mode = CipherMode.ECB;
        aesAlg.Padding = PaddingMode.PKCS7;

        using var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, null);
        var dataBytes = Encoding.UTF8.GetBytes(payload);
        var encryptedBytes = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }
}