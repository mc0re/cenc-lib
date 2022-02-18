using PiffLibrary.Boxes;

namespace CencLibrary;

internal interface ICencBlockCipherFactory
{
    ICencBlockCipher Create(PiffEncryptionTypes cipherType, byte[] key);
}
