using PiffLibrary.Boxes;

namespace CencLibrary;

internal sealed class CencDefaultCipherFactory : ICencBlockCipherFactory
{
    public ICencBlockCipher Create(PiffEncryptionTypes cipherType, byte[] key)
    {
        switch (cipherType)
        {
            case PiffEncryptionTypes.AesCtr:
                return new CencAesBlockCipher(cipherType, key);

            default:
                throw new NotImplementedException();
        }
    }
}
