using System.Security.Cryptography;

namespace CencLibrary;


/// <summary>
/// There is no CTR mode, we use ECB and a counter to implement CTR
/// See https://stackoverflow.com/questions/6374437/can-i-use-aes-in-ctr-mode-in-net
/// </summary>
public class AesCounterMode : SymmetricAlgorithm
{
    private readonly ulong mNonce;
    private readonly ulong mCounter;
    private readonly Aes mAes;

    public AesCounterMode(byte[] nonce, ulong counter)
        : this(ConvertNonce(nonce), counter)
    {
    }


    public AesCounterMode(ulong nonce, ulong counter)
    {
        mAes = Aes.Create();
        mAes.Mode = CipherMode.ECB;
        mAes.Padding = PaddingMode.None;

        mNonce = nonce;
        mCounter = counter;
    }


    private static ulong ConvertNonce(byte[] nonce)
    {
        if (nonce == null) throw new ArgumentNullException(nameof(nonce));
        if (nonce.Length < sizeof(ulong)) throw new ArgumentException($"{nameof(nonce)} must have at least {sizeof(ulong)} bytes");

        return BitConverter.ToUInt64(nonce);
    }


    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? iv)
    {
        return new CounterModeCryptoTransform(mAes, rgbKey, mNonce, mCounter);
    }


    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? iv)
    {
        return new CounterModeCryptoTransform(mAes, rgbKey, mNonce, mCounter);
    }


    public override void GenerateKey()
    {
        mAes.GenerateKey();
    }


    public override void GenerateIV()
    {
        // IV not needed in Counter Mode
    }
}
