using PiffLibrary.Boxes;

namespace CencLibrary;

internal class SampleCursor
{
    private uint mTrackId;
    private PiffSampleTableBox mStbl;
    private int mSampleIndex;
    private int mChunkIndex;
    private uint mSampleCount;

    public bool EndReached { get; private set; }

    public SampleCursor(uint trackId, PiffSampleTableBox stbl)
    {
        mTrackId = trackId;
        mStbl = stbl;

        mSampleCount =
            stbl.FirstOfType<PiffSampleSizeBox>()?.SampleCount ??
            stbl.FirstOfType<PiffCompactSampleSizeBox>()?.SampleCount ??
            0;
        if (mSampleCount == 0)
            EndReached = true;
    }
}