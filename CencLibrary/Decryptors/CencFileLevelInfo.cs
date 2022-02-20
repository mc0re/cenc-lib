using PiffLibrary;
using PiffLibrary.Boxes;

namespace CencLibrary;


internal class CencFileLevelInfo
{
    #region Properties

    public PiffMovieBox Moov { get; private set; }

    public PiffTrackBox[] Tracks { get; private set; }

    public IList<CencTrackDecryptor> Decryptors { get; } = new List<CencTrackDecryptor>();

    #endregion


    #region Init and clean-up

    public CencFileLevelInfo(PiffFile inputFile, CencDecryptionContext ctx, byte[] key)
    {
        Moov = inputFile.GetSingleBox<PiffMovieBox>();
        if (Moov == null)
        {
            ctx.AddError($"No '{PiffReader.GetBoxName<PiffMovieBox>()}' box in the file.");
            return;
        }

        Tracks = Moov.ChildrenOfType<PiffTrackBox>();
        if (Tracks.Length == 0)
        {
            ctx.AddError($"No '{PiffReader.GetBoxName<PiffTrackBox>()}' in the file.");
            return;
        }

        var cursors = new List<SampleCursor>();

        foreach (var track in Tracks)
        {
            var stbl = track.FirstOfType<PIffTrackMediaInfoBox, PiffMediaInformationBox, PiffSampleTableBox>();
            if (stbl == null)
            {
                ctx.AddWarning($"No '{PiffReader.GetBoxName<PiffSampleTableBox>()}' for track.");
                continue;
            }

            var stsd = stbl.FirstOfType<PiffSampleDescriptionBox>();
            if (stsd == null)
            {
                ctx.AddWarning($"No '{PiffReader.GetBoxName<PiffSampleDescriptionBox>()}' for track.");
                continue;
            }

            var samples = stsd.ChildrenOfType<PiffSampleEntryBoxBase>();
            if (!samples.Any())
            {
                ctx.AddWarning("No sample entries for track.");
                continue;
            }

            var trackId = track.FirstOfType<PiffTrackHeaderBox>().TrackId;

            // There is stsd.First<PiffTrackEncryptionBox>().DefaultKeyId to get the key ID
            var decr = new CencTrackDecryptor(samples, trackId, key);
            Decryptors.Add(decr);
            cursors.Add(new SampleCursor(trackId, stbl));
        }

        if (cursors.Any(c => !c.EndReached))
        {
            ctx.AddError("External samples are not suppoted.");
            return;
        }
    }

    #endregion
}
