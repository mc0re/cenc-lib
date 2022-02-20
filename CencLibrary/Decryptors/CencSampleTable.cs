using PiffLibrary.Boxes;

namespace CencLibrary;


internal class CencSampleTable
{
    /// <summary>
    /// Start of the data in the corresponding <see cref="PiffMediaDataBox"/>.
    /// </summary>
    public ulong BaseOffset { get; }

    public List<CencSample> SampleTable { get; }

    public ulong Duration { get; }


    public CencSampleTable(
        PiffMovieBox moov, PiffMovieFragmentBox moof, PiffTrackFragmentBox traf,
        PiffTrackFragmentRunBox[] truns, uint trackId)
    {
        var mvex = moov.FirstOfType<PiffMovieExtendedBox>();
        var trex = mvex?.ChildrenOfType<PiffTrackExtendedBox>()
                        .FirstOrDefault(t => t.TrackId == trackId);

        var tfdt = traf.FirstOfType<PiffTrackFragmentDecodeTimeBox>();
        var timeOffset = tfdt?.DecodeTime ?? 0;
        var tfhd = traf.FirstOfType<PiffTrackFragmentHeaderBox>();
        BaseOffset = (tfhd.Flags & PiffTrackFragmentHeaderBox.FlagsBaseOffsetPresent) != 0
            ? tfhd.BaseDataOffset
            : moof.OriginalPosition;

        var descriptionIndex =
            (tfhd.Flags & PiffTrackFragmentHeaderBox.FlagsDescriptionIndexPresent) != 0 ? tfhd.SampleDescriptionIndex :
            trex?.DefaultDescriptionIndex ?? 0;
        var defaultSize =
            (tfhd.Flags & PiffTrackFragmentHeaderBox.FlagsDefaultSizePresent) != 0 ? tfhd.DefaultSampleSize :
            trex?.DefaultSampleSize ?? 0;
        var defaultDuration =
            (tfhd.Flags & PiffTrackFragmentHeaderBox.FlagsDefaultSizePresent) != 0 ? tfhd.DefaultSampleDuration :
            trex?.DefaultSampleDuration ?? 0;
        var defaultFlags =
            (tfhd.Flags & PiffTrackFragmentHeaderBox.FlagsDefaultFlagsPresent) != 0 ? tfhd.DefaultSampleFlags :
            trex?.DefaultSampleFlags ?? 0;

        var sampleList = new List<CencSample>();

        for (int trunIdx = 0; trunIdx < truns.Length; trunIdx++)
        {
            var trun = truns[trunIdx];
            var dataOffset = (ulong) ((long) BaseOffset + trun.DataOffset);

            // MS hack
            if (dataOffset == moof.OriginalPosition)
            {
                //dataOffset = mdat_body_offset;
            }

            foreach (var sample in trun.Samples)
            {
                var size = (trun.Flags & PiffTrackFragmentRunBox.FlagsSampleSizePresent) != 0 ? sample.Size : defaultSize;
                var duration = (trun.Flags & PiffTrackFragmentRunBox.FlagsSampleDurationPresent) != 0 ? sample.Duration : defaultDuration;
                var flags = (trun.Flags & PiffTrackFragmentRunBox.FlagsFirstSampleFlagPresent) != 0 && trunIdx == 0 ? trun.FirstSampleFlags :
                            (trun.Flags & PiffTrackFragmentRunBox.FlagsSampleFlagsPresent) != 0 ? sample.Flags :
                            defaultFlags;
                var sync = (flags & PiffTrackFragmentRunSample.FlagsSampleIsDifference) == 0;
                var cts = (trun.Flags & PiffTrackFragmentRunBox.FlagsTimeOffsetPresent) != 0 ? sample.TimeOffset : 0;

                var sampleInfo = new CencSample(flags, size, duration, sync,
                    descriptionIndex > 0 ? descriptionIndex - 1 : 0,
                    dataOffset, timeOffset, cts);
                sampleList.Add(sampleInfo);

                dataOffset += size;
                timeOffset += duration;
            }
        }

        SampleTable = sampleList;
        Duration = timeOffset;
    }
}
