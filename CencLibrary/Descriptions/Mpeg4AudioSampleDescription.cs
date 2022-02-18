using PiffLibrary.Boxes;

namespace CencLibrary;

internal class Mpeg4AudioSampleDescription : SampleDescription
{
    private string mFormat;
    private uint mSampleRate;
    private ushort mSampleSize;
    private ushort mChannelCount;
    private PiffElementaryStreamDescriptionBox mEsdsBox;


    public Mpeg4AudioSampleDescription(
        string format, uint sampleRate, ushort sampleSize, ushort channelCount,
        PiffElementaryStreamDescriptionBox esds) :
        base(SampleDescriptionTypes.Mpeg, format)
    {
        mFormat = format;
        mSampleRate = sampleRate;
        mSampleSize = sampleSize;
        mChannelCount = channelCount;
        mEsdsBox = esds;
    }
}