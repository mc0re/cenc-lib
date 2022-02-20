using PiffLibrary;
using PiffLibrary.Boxes;

namespace CencLibrary;


public class CencDecryptor
{
    public int Decrypt(Stream input, Stream output, byte[] key)
    {
        var rctx = new PiffReadContext();

        var inputFile = PiffFile.Parse(input, rctx);
        if (rctx.Messages.Any())
        {
            foreach (var msg in rctx.Messages)
                Console.WriteLine(msg);
            if (rctx.IsError) return -1;
        }

        var dctx = new CencDecryptionContext();
        var fileInfo = new CencFileLevelInfo(inputFile, dctx, key);
        if (dctx.Messages.Any())
        {
            foreach (var msg in dctx.Messages)
                Console.WriteLine(msg);
            if (dctx.IsError) return -1;
        }

        foreach (var decr in fileInfo.Decryptors)
        {
            decr.ChangeFormat();
        }

        var wctx = new PiffWriteContext();
        for (int boxIdx = 0; boxIdx < inputFile.Boxes.Count; boxIdx++)
        {
            var box = inputFile.Boxes[boxIdx];
            if (box.OriginalPosition != (ulong) output.Position)
            {
                wctx.AddError($"Box {box.BoxType} started at {box.OriginalPosition}, now written at {output.Position}.");
                break;
            }

            PiffWriter.WriteBox(output, box, wctx);

            if (box is not PiffMovieFragmentBox moof) continue;

            // moof is followed by mdat, which it describes
            boxIdx++;
            var nextBox = inputFile.Boxes[boxIdx];
            if (nextBox is not PiffMediaDataBox mdat)
            {
                wctx.AddError($"'{PiffReader.GetBoxName<PiffMediaDataBox>()}' expected, found '{box.BoxType}' at {box.OriginalPosition}.");
                continue;
            }

            if (mdat.OriginalPosition != (ulong) output.Position)
            {
                wctx.AddError($"Box {mdat.BoxType} started at {mdat.OriginalPosition}, now written at {output.Position}.");
                break;
            }

            PiffWriter.WriteBoxHeader(output, mdat.BoxType, mdat.OriginalSize);

            // One of each per track fragment
            var handlers = new List<CencFragmentDecryptor>();
            var sampleTables = new List<CencSampleTable>();

            foreach (var traf in moof.ChildrenOfType<PiffTrackFragmentBox>())
            {
                var truns = traf.ChildrenOfType<PiffTrackFragmentRunBox>();
                var handler = new CencFragmentDecryptor(traf, truns, fileInfo.Tracks, fileInfo.Moov, fileInfo.Decryptors, dctx);
                if (dctx.Messages.Any()) continue;

                handlers.Add(handler);
                sampleTables.Add(new CencSampleTable(fileInfo.Moov, moof, traf, truns, handler.TrackId));
            }

            for (int trafIdx = 0; trafIdx < sampleTables.Count; trafIdx++)
            {
                var sampleTable = sampleTables[trafIdx];
                var handler = handlers[trafIdx];

                for (int sampleIdx = 0; sampleIdx < sampleTable.SampleTable.Count; sampleIdx++)
                {
                    var sample = sampleTable.SampleTable[sampleIdx];

                    input.Seek((long) sample.DataOffset, SeekOrigin.Begin);
                    var encData = new byte[sample.Size];
                    input.Read(encData, 0, encData.Length);

                    handler.Decrypt(encData, output, sampleIdx);
                }
            }
        }

        if (wctx.Errors.Any())
        {
            foreach (var msg in wctx.Errors)
                Console.WriteLine(msg);
        }

        return inputFile.Boxes.Count;
    }
}
