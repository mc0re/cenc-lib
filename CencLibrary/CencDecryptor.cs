using PiffLibrary;

namespace CencLibrary;


public class CencDecryptor
{
    public void Decrypt(Stream input, Stream output, byte[] key)
    {
        var inputFile = PiffFile.Parse(input);
    }
}
