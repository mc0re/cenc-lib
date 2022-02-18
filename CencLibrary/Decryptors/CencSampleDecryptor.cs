using PiffLibrary;
using PiffLibrary.Boxes;

namespace CencLibrary;


internal sealed class CencSampleDecryptor
{

    public PiffProtectionTrackEncryption Tenc { get; }

    public PiffSampleEncryption Senc { get; }

    public byte PerSampleIvSize { get; }

    public byte IvSize { get; }

    public bool ResetIv { get; }

    public CencSingleSampleDecryptor SingleDecryptor { get; }


    public CencSampleDecryptor(
        ProtectedSampleDescription sampleDescription, PiffTrackFragmentBox traf, byte[] trackKey, CencDecryptionContext ctx)
    {
        var tenc = sampleDescription.SchiBox.FirstOfType<PiffTrackEncryptionBox>()?.Data ??
                   sampleDescription.SchiBox.ChildrenOfType<PiffExtensionBox>()
                                     .FirstOrDefault(b => b.BoxId == PiffProtectionTrackEncryption.BoxId)?
                                     .Track;
        if (tenc is null)
        {
            ctx.AddError($"No '{PiffReader.GetBoxName<PiffTrackEncryptionBox>()}' for track.");
            return;
        }

        Tenc = tenc;

        var senc = traf.FirstOfType<PiffSampleEncryptionBox>()?.Data ??
                   traf.ChildrenOfType<PiffExtensionBox>()
                       .FirstOrDefault(b => b.BoxId == PiffSampleEncryption.BoxId)?
                       .Sample;
        if (senc is null)
        {
            ctx.AddError($"No '{PiffReader.GetBoxName<PiffSampleEncryptionBox>()}' for track.");
            return;
        }

        Senc = senc;

        PiffEncryptionTypes cipherType;

        if ((senc.ParentFlags & PiffSampleEncryption.OverrideTrack) != 0)
        {
            cipherType = senc.Algorithm.AlgorithmId;
            PerSampleIvSize = senc.Algorithm.InitVectorSize;
        }
        else
        {
            switch (sampleDescription.SchemeType)
            {
                case "piff":
                    cipherType = tenc.DefaultAlgorithmId;
                    break;

                case "cenc" or "cens":
                    cipherType = PiffEncryptionTypes.AesCtr;
                    break;

                case "cbc1":
                    cipherType = PiffEncryptionTypes.AesCbc;
                    break;

                case "cbcs":
                    cipherType = PiffEncryptionTypes.AesCbc;
                    ResetIv = true;
                    break;

                default:
                    ctx.AddError($"Encryption '{sampleDescription.SchemeType}' is not supported.");
                    return;
            }

            PerSampleIvSize = tenc.DefaultPerSampleIvSize;

            if (tenc.DefaultAlgorithmId == PiffEncryptionTypes.NoEncryption)
                cipherType = PiffEncryptionTypes.NoEncryption;
        }

        SingleDecryptor = new CencSingleSampleDecryptor(cipherType, trackKey);

        IvSize = PerSampleIvSize > 0 ? PerSampleIvSize : Tenc.ConstantInitVectorSize;
        if (IvSize == 0)
        {
            ctx.AddError($"IV size is not defined.");
        }
    }
}
