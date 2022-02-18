namespace CencLibrary;

internal sealed class CencSample
{
    public uint Flags { get; }

    public uint Size { get; }

    public uint Duration { get; }

    public bool Sync { get; }

    public uint DescriptionIndex { get; }

    public ulong DataOffset { get; }

    public ulong TimeOffset { get; }

    public long Cts { get; }


    public CencSample(
        uint flags, uint size, uint duration, bool sync, uint descriptionIndex, ulong dataOffset, ulong timeOffset, long cts)
    {
        Flags = flags;
        Size = size;
        Duration = duration;
        Sync = sync;
        DescriptionIndex = descriptionIndex;
        DataOffset = dataOffset;
        TimeOffset = timeOffset;
        Cts = cts;
    }
}