using PiffLibrary.Boxes;

namespace CencLibrary;


/// <summary>
/// Decrypts the subsamples of the given sample data.
/// </summary>
internal sealed class CencSampleDecryptor
{
    #region Fields

    /// <summary>
    /// Whether to set the IV back to the given value after decoding each sub-sample.
    /// </summary>
    private readonly bool mResetIv;

    private readonly ICencBlockCipher? mCipher;

    #endregion


    #region Init and clean-up

    public CencSampleDecryptor(PiffEncryptionTypes cipherType, byte[] trackKey, bool resetIv)
    {
        ICencBlockCipherFactory factory = new CencDefaultCipherFactory();

        switch (cipherType)
        {
            case PiffEncryptionTypes.NoEncryption:
                mCipher = null;
                break;

            case PiffEncryptionTypes.AesCtr:
                mCipher = factory.Create(cipherType, trackKey);
                break;

            default:
                throw new NotImplementedException();
        }

        mResetIv = resetIv;
    }

    #endregion


    #region API

    public void Decrypt(byte[] encData, Stream output, byte[] iv, PiffSampleEncryptionSubSample[] subSamples)
    {
        if (mCipher is null)
        {
            output.Write(encData, 0, encData.Length);
            return;
        }

        mCipher.SetIv(iv);

        if (subSamples.Any())
            DecryptSubSamples(encData, output, iv, subSamples);
        else
            DecryptWholeSample(encData, output);
    }

    #endregion


    #region Utility

    private void DecryptSubSamples(byte[] encData, Stream output, byte[] iv, PiffSampleEncryptionSubSample[] subSamples)
    {
        var position = 0;

        foreach (var subSample in subSamples)
        {
            if (subSample.ClearDataSize > 0)
            {
                output.Write(encData, 0, subSample.ClearDataSize);
                position += subSample.ClearDataSize;
            }

            if (subSample.EncryptedDataSize > 0)
            {
                if (mResetIv)
                {
                    mCipher.SetIv(iv);
                }

                mCipher.Decode(encData, position, subSample.EncryptedDataSize, output);
                position += (int) subSample.EncryptedDataSize;
            }
        }

        if (position < encData.Length)
        {
            output.Write(encData, position, encData.Length - position);
        }
    }


    private void DecryptWholeSample(byte[] encData, Stream output)
    {
        mCipher.Decode(encData, 0, (uint) encData.Length, output);        
    }

    #endregion
}
