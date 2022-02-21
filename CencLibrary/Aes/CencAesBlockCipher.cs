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

    private ICryptoTransform? mAes;


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
        Array.Fill<byte>(mCounter, 0);
        mAes?.Dispose();
        mAes = null;
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
