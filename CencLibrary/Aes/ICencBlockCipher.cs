namespace CencLibrary;


internal interface ICencBlockCipher : IDisposable
{
    void SetIv(byte[] iv);

    void Decode(byte[] encData, int position, uint dataSize, Stream output);
}