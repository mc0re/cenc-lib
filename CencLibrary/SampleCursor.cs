using PiffLibrary.Boxes;

namespace CencLibrary;

internal class SampleCursor
{
    private uint mTrackId;
    private PiffSampleTableBox mStbl;
    private int mSampleIndex;
    private int mChunkIndex;
    private Stream mInput;
    private uint mSampleCount;

    public bool EndReached { get; private set; }

    public SampleCursor(uint trackId, PiffSampleTableBox stbl, Stream input)
    {
        mTrackId = trackId;
        mStbl = stbl;
        mInput = input;

        mSampleCount =
            stbl.FirstOfType<PiffSampleSizeBox>()?.SampleCount ??
            stbl.FirstOfType<PiffCompactSampleSizeBox>()?.SampleCount ??
            0;
        if (mSampleCount == 0)
            EndReached = true;
    }
}