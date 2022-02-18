using PiffLibrary;
using PiffLibrary.Boxes;

namespace CencLibrary;


public class CencDecryptor
{
    public int Decrypt(Stream input, Stream output, byte[] key)
    {
        var rctx = new PiffReadContext();
        var dctx = new CencDecryptionContext();

        var inputFile = PiffFile.ParseButSkipData(input, rctx);
        if (rctx.Messages.Any())
        {
            foreach (var msg in rctx.Messages)
                Console.WriteLine(msg);
        }

        var moov = inputFile.GetSingleBox<PiffMovieBox>();
        if (moov == null)
            return dctx.AddError($"No '{PiffReader.GetBoxName<PiffMovieBox>()}' box in the file.");

        var tracks = moov.ChildrenOfType<PiffTrackBox>();
        if (tracks.Length == 0)
            return dctx.AddError($"No '{PiffReader.GetBoxName<PiffTrackBox>()}' in the file.");

        var decryptors = new List<CencTrackDecryptor>();
        var cursors = new List<SampleCursor>();

        foreach (var track in tracks)
        {
            var stbl = track.FirstOfType<PIffTrackMediaInfoBox, PiffMediaInformationBox, PiffSampleTableBox>();
            if (stbl == null)
            {
                dctx.AddWarning($"No '{PiffReader.GetBoxName<PiffSampleTableBox>()}' for track.");
                continue;
            }

            var stsd = stbl.FirstOfType<PiffSampleDescriptionBox>();
            if (stsd == null)
            {
                dctx.AddWarning($"No '{PiffReader.GetBoxName<PiffSampleDescriptionBox>()}' for track.");
                continue;
            }

            var samples = stsd.ChildrenOfType<PiffSampleEntryBoxBase>();
            if (!samples.Any())
            {
                dctx.AddWarning("No sample entries for track.");
                continue;
            }

            var trackId = track.FirstOfType<PiffTrackHeaderBox>().TrackId;

            // There is stsd.First<PiffTrackEncryptionBox>().DefaultKeyId to get the key ID
            var decr = new CencTrackDecryptor(samples, trackId, key);
            decryptors.Add(decr);
            cursors.Add(new SampleCursor(trackId, stbl, input));
        }

        if (cursors.Any(c => !c.EndReached))
            return dctx.AddError("External samples are not suppoted.");

        foreach (var decr in decryptors)
        {
            decr.ChangeFormat();
        }

        var wctx = new PiffWriteContext();
        for (int boxIdx = 0; boxIdx < inputFile.Boxes.Count; boxIdx++)
        {
            var box = inputFile.Boxes[boxIdx];
            if (box.StartOffset != (ulong) output.Position)
            {
                wctx.AddError($"Box {box.BoxType} started at {box.StartOffset}, now written at {output.Position}.");
                break;
            }

            PiffWriter.WriteBox(output, box, wctx);

            if (box is not PiffMovieFragmentBox moof) continue;

            // moof is followed by mdat, which it describes
            boxIdx++;
            var nextBox = inputFile.Boxes[boxIdx];
            if (nextBox is not PiffMediaDataBox mdat)
            {
                wctx.AddError($"'{PiffReader.GetBoxName<PiffMediaDataBox>()}' expected, found '{box.BoxType}' at {box.StartOffset}.");
                continue;
            }

            var handlers = new List<CencFragmentDecryptor>();
            var sampleTables = new List<CencSampleTable>();

            foreach (var traf in moof.ChildrenOfType<PiffTrackFragmentBox>())
            {
                var truns = traf.ChildrenOfType<PiffTrackFragmentRunBox>();
                var handler = new CencFragmentDecryptor(traf, truns, tracks, moov, decryptors, dctx);
                if (dctx.Messages.Any()) continue;

                handlers.Add(handler);
                sampleTables.Add(new CencSampleTable(moov, moof, traf, truns, handler.TrackId));
            }

            PiffWriter.WriteBoxHeader(output, mdat, wctx);
        }

        if (wctx.Errors.Any())
        {
            foreach (var msg in wctx.Errors)
                Console.WriteLine(msg);
        }

        return inputFile.Boxes.Count;
    }
}
