using System.Security.Cryptography;
using PiffLibrary.Boxes;

namespace CencLibrary;


/// <summary>
/// A layer on top of the AES cryptoengine.
/// </summary>
internal sealed class CencAesBlockCipher : ICencBlockCipher
{
    private readonly byte[] mKey;
    
    private byte[] mIv = new byte[8];

    private readonly byte[] mCounter = new byte[8];

    private ICryptoTransform mAes;


    public CencAesBlockCipher(PiffEncryptionTypes cipherType, byte[] key)
    {
        mKey = key;
    }


    public void Dispose()
    {
        mAes?.Dispose();
    }


    public void SetIv(byte[] iv)
    {
        mIv = iv;
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


    public void Decode(byte[] encData, int position, uint dataSize, Stream output)
    {
        if (mAes is null)
        {
            mAes = new AesCounterMode(mCounter).CreateDecryptor(mKey, mIv);
        }

        var decData = new byte[dataSize];
        var decSize = mAes.TransformBlock(encData, position, (int) dataSize, decData, 0);

        output.Write(decData, 0, decSize);
    }
}
