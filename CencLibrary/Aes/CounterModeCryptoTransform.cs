using System.Security.Cryptography;

namespace CencLibrary;

internal class CounterModeCryptoTransform : ICryptoTransform
{
    private readonly byte[] mNonceAndCounter;
    private readonly ICryptoTransform mCounterEncryptor;
    private readonly Queue<byte> mXorMask = new Queue<byte>();
    private readonly SymmetricAlgorithm mAes;

    private ulong mCounter;
    private byte[] mCounterModeBlock;


    public int InputBlockSize => mAes.BlockSize / 8;
    public int OutputBlockSize => mAes.BlockSize / 8;
    public bool CanTransformMultipleBlocks => true;
    public bool CanReuseTransform => false;


    public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, ulong nonce, ulong counter)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        mAes = symmetricAlgorithm ?? throw new ArgumentNullException(nameof(symmetricAlgorithm));
        mCounter = counter;
        mNonceAndCounter = new byte[16];
        BitConverter.TryWriteBytes(mNonceAndCounter, nonce);
        BitConverter.TryWriteBytes(new Span<byte>(mNonceAndCounter, sizeof(ulong), sizeof(ulong)), counter);

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

        foreach (var b in mCounterModeBlock)
        {
            mXorMask.Enqueue(b);
        }
    }


    private void IncrementCounter()
    {
        mCounter++;
        var span = new Span<byte>(mNonceAndCounter, sizeof(ulong), sizeof(ulong));
        BitConverter.TryWriteBytes(span, mCounter);
    }


    public void Dispose()
    {
        mCounterEncryptor.Dispose();
    }
}
