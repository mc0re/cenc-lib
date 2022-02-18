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

    public byte[] TrackKey { get; }

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
            var sinf = sample.FirstOfType<PiffProtectionSchemeInformationBox>();
            originalFormat = sinf.FirstOfType<PiffOriginalFormatBox>()?.Format ?? originalFormat;

            var schi = sinf.FirstOfType<PiffSchemeInformationBox>();
            var schm = sinf.FirstOfType<PiffSchemeTypeBox>();
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
        TrackKey = trackKey;
        OriginalFormat = originalFormat;
    }

    #endregion


    #region API
    
    /// <summary>
    /// Change the format to the original format, hide the protection information.
    /// </summary>
    public void ChangeFormat()
    {
        foreach (var info in mInfos)
        {
            info.Box.BoxType = OriginalFormat;
            info.Box.Children.OfType<PiffProtectionSchemeInformationBox>().First().BoxType = "skip";
        }
    }


    /// <summary>
    /// 1-based
    /// </summary>
    public ProtectedSampleDescription? GetSampleDescription(uint index)
    {
        if (index < 1 || index > mInfos.Count) return null;
        return mInfos[(int)index - 1].Description;
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
                    var avcc = sample.FirstOfType<PiffAvcConfigurationBox>();
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
                    var esds = sample.FirstOfType<PiffElementaryStreamDescriptionBox>();
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
