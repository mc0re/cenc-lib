using PiffLibrary.Boxes;

namespace CencLibrary;

internal sealed class CencSingleSampleDecryptor
{
    private CencStreamCipher mCipher;


    public CencSingleSampleDecryptor(PiffEncryptionTypes cipherType, byte[] trackKey)
    {
        ICencBlockCipherFactory factory = new CencDefaultCipherFactory();

        switch (cipherType)
        {
            case PiffEncryptionTypes.AesCtr:
                mCipher = new CencStreamCipher(factory.Create(cipherType, trackKey), 8);
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
