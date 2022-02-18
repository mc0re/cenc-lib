using PiffLibrary.Boxes;

namespace CencLibrary;

internal sealed class AvcSampleDescription : SampleDescription
{
    #region Properties

    public ushort Width { get; }
    
    public ushort Height { get; }
    
    public ushort Depth { get; }
    
    public byte[] CompressorName { get; }
    
    public PiffAvcConfigurationBox AvcConfigBox { get; }

    #endregion


    #region Init and clean-up

    public AvcSampleDescription(
        string format, ushort width, ushort height, ushort depth,
        byte[] compressorName, PiffAvcConfigurationBox avcc) :
        base(SampleDescriptionTypes.Avc, format)
    {
        Width = width;
        Height = height;
        Depth = depth;
        CompressorName = compressorName;
        AvcConfigBox = avcc;
    }

    #endregion
}
