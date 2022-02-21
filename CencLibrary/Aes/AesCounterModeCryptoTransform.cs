using System.Security.Cryptography;

namespace CencLibrary;


internal class AesCounterModeCryptoTransform : ICryptoTransform
{
    #region Fields

    private const int IvLength = 8;
    private const int CounterLength = 8;

    private readonly byte[] mNonceAndCounter;
    private readonly ICryptoTransform mCounterEncryptor;
    private readonly Queue<byte> mXorMask = new Queue<byte>();
    private readonly SymmetricAlgorithm mAes;
    private readonly byte[] mCounter;

    private byte[] mCounterModeBlock;

    #endregion


    public int InputBlockSize => mAes.BlockSize / 8;
    public int OutputBlockSize => mAes.BlockSize / 8;
    public bool CanTransformMultipleBlocks => true;
    public bool CanReuseTransform => false;


    public AesCounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[]? iv, byte[] counter)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (iv is null) throw new ArgumentNullException(nameof(iv));
        if (iv.Length != IvLength) throw new ArgumentException($"Must be {IvLength} bytes", nameof(iv));
        if (counter.Length != CounterLength) throw new ArgumentException($"Must be {CounterLength} bytes", nameof(counter));

        mAes = symmetricAlgorithm ?? throw new ArgumentNullException(nameof(symmetricAlgorithm));
        mCounter = counter;
        mNonceAndCounter = new byte[IvLength + CounterLength];
        Array.Copy(iv, mNonceAndCounter, IvLength);
        Array.Copy(counter, 0, mNonceAndCounter, IvLength, CounterLength);

        var zeroIv = new byte[mAes.BlockSize / 8];
        mCounterEncryptor = symmetricAlgorithm.CreateEncryptor(key, zeroIv);
    }


    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        var output = new byte[inputCount];
        TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
        return output;
    }


    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
        int outputOffset)
    {
        for (var i = 0; i < inputCount; i++)
        {
            if (NeedMoreXorMaskBytes())
            {
                EncryptCounterThenIncrement();
            }

            var mask = mXorMask.Dequeue();
            outputBuffer[outputOffset + i] = (byte) (inputBuffer[inputOffset + i] ^ mask);
        }

        return inputCount;
    }


    private bool NeedMoreXorMaskBytes()
    {
        return mXorMask.Count == 0;
    }


    private void EncryptCounterThenIncrement()
    {
        mCounterModeBlock ??= new byte[mAes.BlockSize / 8];

        mCounterEncryptor.TransformBlock(mNonceAndCounter, 0, mNonceAndCounter.Length, mCounterModeBlock, 0);
        IncrementCounter();
        Array.Copy(mCounter, 0, mNonceAndCounter, IvLength, CounterLength);

        foreach (var b in mCounterModeBlock)
        {
            mXorMask.Enqueue(b);
        }
    }


    private void IncrementCounter()
    {
        for (var byteIdx = mCounter.Length - 1; byteIdx >= 0; byteIdx--)
        {
            var b = mCounter[byteIdx];
            if (b < 0xFF)
            {
                mCounter[byteIdx] = (byte) (b + 1);
                break;
            }

            mCounter[byteIdx] = 0;
        }
    }


    public void Dispose()
    {
        mCounterEncryptor.Dispose();
    }
}
