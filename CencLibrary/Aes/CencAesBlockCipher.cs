using System.Security.Cryptography;
using PiffLibrary.Boxes;

namespace CencLibrary;


internal sealed class CencAesBlockCipher : ICencBlockCipher
{
    private readonly ICryptoTransform mAes;
    private readonly byte[] mKey;


    public CencAesBlockCipher(PiffEncryptionTypes cipherType, byte[] key)
    {
        var nonce = new byte[8];
        mAes = new AesCounterMode(nonce, 0).CreateDecryptor(key, null);
        mKey = key;
    }


    public void Dispose()
    {
        mAes.Dispose();
    }


    public void Decrypt()
    {
        //aes.Key = keyAes;
        //aes.IV = iv;

        //using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        //using (var resultStream = new MemoryStream())
        //{
        //    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
        //    using (var plainStream = new MemoryStream(buffer))
        //    {
        //        plainStream.CopyTo(aesStream);
        //    }

        //    result = resultStream.ToArray();
        //}

        //    var counter = 0;
        //    using var counterMode = new AesCounterMode(nonce, counter);
        //    using var decryptor = counterMode.CreateDecryptor(key, null);
        //
        //    var decrypted = new byte[dataToEncrypt.Length];
        //    decryptor.TransformBlock(encryptedData, 0, bytesWritten, decrypted, 0);
    }
}
