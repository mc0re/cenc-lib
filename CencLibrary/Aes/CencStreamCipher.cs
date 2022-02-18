namespace CencLibrary;

internal class CencStreamCipher
{
    private readonly ICencBlockCipher mBlockCipher;
    private readonly int mCounterSize;


    public CencStreamCipher(ICencBlockCipher blockCipher, int counterSize)
    {
        mBlockCipher = blockCipher;
        mCounterSize = Math.Min(counterSize, 16);
    }
}
