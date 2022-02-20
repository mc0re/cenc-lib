using System.Security.Cryptography;

namespace CencLibrary;


/// <summary>
/// There is no CTR mode, we use ECB and a counter to implement CTR
/// See https://stackoverflow.com/questions/6374437/can-i-use-aes-in-ctr-mode-in-net
/// </summary>
public class AesCounterMode : SymmetricAlgorithm
{
    #region Fields

    private readonly byte[] mCounter;

    private readonly Aes mAes;

    #endregion


    #region Init and clean-up

    /// <summary>
    /// Create an AES transformer.
    /// </summary>
    /// <param name="counter">A counter, gets modified during the transformation process</param>
    public AesCounterMode(byte[] counter)
    {
        mAes = Aes.Create();
        mAes.Mode = CipherMode.ECB;
        mAes.Padding = PaddingMode.None;

        mCounter = counter;
    }

    #endregion


    #region Overrides

    /// <inheritdoc/>
    public override ICryptoTransform CreateEncryptor(byte[] key, byte[]? iv)
    {
        return new AesCounterModeCryptoTransform(mAes, key, iv, mCounter);
    }



    /// <inheritdoc/>
    public override ICryptoTransform CreateDecryptor(byte[] key, byte[]? iv)
    {
        return new AesCounterModeCryptoTransform(mAes, key, iv, mCounter);
    }


    /// <inheritdoc/>
    public override void GenerateKey()
    {
        mAes.GenerateKey();
    }


    /// <inheritdoc/>
    public override void GenerateIV()
    {
        // IV not needed in Counter Mode
    }

    #endregion
}
