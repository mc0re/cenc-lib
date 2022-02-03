using PiffLibrary;
using PiffLibrary.Boxes;

namespace CencLibrary;


public class CencDecryptor
{
    public int Decrypt(Stream input, Stream output, byte[] key)
    {
        var inputFile = PiffFile.ParseButSkipData(input);

        var moov = inputFile.GetSingleBox<PiffMovieBox>();
        if (moov == null)
            throw new ArgumentException("No 'moov' box in the file.");

        var tracks = moov.OfType<PiffTrackBox>();
        if (tracks.Length == 0)
            throw new ArgumentException("No tracks in the file.");

        var decryptors = new List<CencTrackDecryptor>();
        var cursors = new List<SampleCursor>();

        foreach (var track in tracks)
        {
            var stbl = track.First<PIffTrackMediaInfoBox, PiffMediaInformationBox, PiffSampleTableBox>();
            if (stbl == null) continue;

            var stsd = stbl.First<PiffSampleDescriptionBox>();
            if (stsd == null) continue;

            var samples = stsd.OfType<PiffSampleEntryBoxBase>();
            if (!samples.Any()) continue;

            var trackId = track.First<PiffTrackHeaderBox>().TrackId;
            // Multiple keys: use trackId or stsd.First<PiffTrackEncryptionBox>().DefaultKeyId
            var trackKey = key;

            var decr = new CencTrackDecryptor(samples, trackId, trackKey);
            decryptors.Add(decr);
            cursors.Add(new SampleCursor(trackId, stbl, input));
        }
        var c = cursors.ToArray();

        return inputFile.Boxes.Count;
    }
}

internal class SampleCursor
{
    private uint mTrackId;
    private PiffSampleTableBox mStbl;
    private int mSampleIndex;
    private int mChunkIndex;
    private Stream mInput;
    private uint mSampleCount;
    private bool mEndReached;


    public SampleCursor(uint trackId, PiffSampleTableBox stbl, Stream input)
    {
        mTrackId = trackId;
        mStbl = stbl;
        mInput = input;

        mSampleCount = stbl.First<PiffSampleSizeBox>()?.SampleCount ?? stbl.First<PiffCompactSampleSizeBox>()?.SampleCount ?? 0;
        if (mSampleCount > 0)
            mEndReached = true;
    }
}