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

        var cursors = new List<SampleCursor>();

        foreach (var track in tracks)
        {
            var stbl = track.First<PIffTrackMediaInfoBox, PiffMediaInformationBox, PiffSampleTableBox>();
            if (stbl == null) continue;

            var stsd = stbl.First<PiffSampleDescriptionBox>();
            if (stsd == null) continue;

            foreach (var sample in stsd.OfType<PiffSampleEntryBoxBase>())
            {
                var decr = new CencTrackDecryptor(sample);
            }
        }
        var c = cursors.ToArray();

        return inputFile.Boxes.Count;
    }
}

internal class CencTrackDecryptor
{
    public CencTrackDecryptor(PiffSampleEntryBoxBase sample)
    {
        var sinf = sample.First<PiffProtectionSchemeInformationBox>();
        var frma = sinf.First<PiffOriginalFormatBox>();
        var schi = sinf.First<PiffSchemeInformationBox>();
        var schm = sinf.First<PiffSchemeTypeBox>();

        // Original format:
        // - avc1, avc2, avc3, avc4, dvav, dva1 - avcC
        // - hvc1, hev1, dvhe, dvh1 - HEVC
        // - mp4v - MPEG Video
        if (schm != null)
        {
            var avcc = sinf.First<PiffAvcConfigurationBox>();
        }
    }
}

internal class SampleCursor
{

}