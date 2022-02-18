namespace CencLibrary;

internal abstract class SampleDescription
{
    #region Properties

    public SampleDescriptionTypes Type { get; }

    public string Format { get; }

    #endregion


    #region Init and clean-up

    public SampleDescription(SampleDescriptionTypes type, string format)
    {
        Type = type;
        Format = format;
    }

    #endregion
}
