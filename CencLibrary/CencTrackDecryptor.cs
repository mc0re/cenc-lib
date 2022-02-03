using PiffLibrary.Boxes;

namespace CencLibrary;

internal sealed class CencTrackDecryptor
{
    #region Constants

    private const string DefaultVideoFormat = "mp4v";
    
    private const string DefaultAudioFormat = "mp4a";

    private static readonly string[] mKnownProtections = { "piff", "cenc", "cens", "cbc1", "cbcs" };

    #endregion


    #region Fields

    private record struct SampleInfo(ProtectedSampleDescription Description, PiffSampleEntryBoxBase Box);

    private readonly IList<SampleInfo> mInfos = new List<SampleInfo>();

    #endregion


    #region Properties

    public uint TrackId { get; }

    public string OriginalFormat { get; }

    #endregion


    #region Init and clean-up
    
    public CencTrackDecryptor(IEnumerable<PiffSampleEntryBoxBase> samples, uint trackId, byte[] trackKey)
    {
        string originalFormat = samples.First() switch
        {
            PiffVideoSampleEntryBox => DefaultVideoFormat,
            PiffAudioSampleEntryBox => DefaultAudioFormat,
            _ => throw new ArgumentException("Unsupported sample type")
        };

        foreach (var sample in samples)
        {
            var sinf = sample.First<PiffProtectionSchemeInformationBox>();
            originalFormat = sinf.First<PiffOriginalFormatBox>()?.Format ?? originalFormat;

            var schi = sinf.First<PiffSchemeInformationBox>();
            var schm = sinf.First<PiffSchemeTypeBox>();
            var psd = sample switch
            {
                PiffVideoSampleEntryBox v => CreateVideoDescription(schm, originalFormat, v, schi),
                PiffAudioSampleEntryBox a => CreateAudioDescription(schm, originalFormat, a, schi),
                _ => throw new ArgumentException("Unsupported sample type")
            };

            if (!mKnownProtections.Contains(psd.SchemeType))
                throw new ArgumentException($"Unsupported protection scheme '{psd.SchemeType}'.");

            mInfos.Add(new SampleInfo(psd, sample));
        }

        TrackId = trackId;
        OriginalFormat = originalFormat;
    }

    #endregion


    #region Utility

    private ProtectedSampleDescription CreateVideoDescription(
        PiffSchemeTypeBox schm, string originalFormat, PiffVideoSampleEntryBox sample, PiffSchemeInformationBox schi)
    {
        // Original format:
        // - hvc1, hev1, dvhe, dvh1 - HEVC
        // - mp4v - MPEG Video
        if (schm != null)
        {
            switch (originalFormat)
            {
                case "avc1":
                case "avc2":
                case "avc3":
                case "avc4":
                case "dvav":
                case "dva1":
                    var avcc = sample.First<PiffAvcConfigurationBox>();
                    var sd = new AvcSampleDescription(
                        originalFormat, sample.Width, sample.Height, sample.Depth, sample.CompressorName, avcc);
                    return new ProtectedSampleDescription(
                        sample.BoxType, sd, originalFormat, schm.SchemeType, schm.SchemeVersion, schm.SchemeUrl, schi);

                default:
                    throw new NotImplementedException($"Unsupported format '{originalFormat}'.");
            }
        }
        else
        {
            throw new NotImplementedException("Scenario schm=null not implemented. 'odkm' box may mean OMA.");
        }
    }


    private ProtectedSampleDescription CreateAudioDescription(
        PiffSchemeTypeBox schm, string originalFormat, PiffAudioSampleEntryBox sample, PiffSchemeInformationBox schi)
    {
        if (schm != null)
        {
            switch (originalFormat)
            {
                case "mp4a":
                    var esds = sample.First<PiffElementaryStreamDescriptionMp4aBox>();
                    var sd = new Mpeg4AudioSampleDescription(
                        originalFormat, sample.SampleRate >> 16, sample.SampleSize, sample.ChannelCount, esds);
                    return new ProtectedSampleDescription(
                        sample.BoxType, sd, originalFormat, schm.SchemeType, schm.SchemeVersion, schm.SchemeUrl, schi);

                default:
                    throw new NotImplementedException($"Unsupported format '{originalFormat}'.");
            }
        }
        else
        {
            throw new NotImplementedException("Scenario schm=null not implemented. 'odkm' box may mean OMA.");
        }
    }

    #endregion
}

internal class Mpeg4AudioSampleDescription : SampleDescription
{
    private string mFormat;
    private uint mSampleRate;
    private ushort mSampleSize;
    private ushort mChannelCount;
    private PiffElementaryStreamDescriptionMp4aBox mEsdsBox;


    public Mpeg4AudioSampleDescription(
        string format, uint sampleRate, ushort sampleSize, ushort channelCount,
        PiffElementaryStreamDescriptionMp4aBox esds) :
        base(SampleDescriptionTypes.Mpeg, format)
    {
        mFormat = format;
        mSampleRate = sampleRate;
        mSampleSize = sampleSize;
        mChannelCount = channelCount;
        mEsdsBox = esds;
    }
}