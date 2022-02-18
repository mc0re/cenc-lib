using PiffLibrary.Boxes;

namespace CencLibrary;


internal sealed class ProtectedSampleDescription : SampleDescription
{
    #region Properties

    public SampleDescription OriginalDescription { get; }

    public string OriginalFormat { get; }

    public string SchemeType { get; }

    public uint SchemeVersion { get; }

    public string SchemeUrl { get; }

    public PiffSchemeInformationBox SchiBox { get; }

    #endregion


    #region Init and clean-up

    public ProtectedSampleDescription(
        string protectedFormat, SampleDescription originalDescription, string originalFormat,
        string schemeType, uint schemeVersion, string schemeUrl, PiffSchemeInformationBox schi) :
        base(SampleDescriptionTypes.Protected, protectedFormat)
    {
        OriginalDescription = originalDescription;
        OriginalFormat = originalFormat;
        SchemeType = schemeType;
        SchemeVersion = schemeVersion;
        SchemeUrl = schemeUrl;
        SchiBox = schi;
    }

    #endregion
}
