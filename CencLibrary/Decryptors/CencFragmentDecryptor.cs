using PiffLibrary;
using PiffLibrary.Boxes;

namespace CencLibrary;


internal class CencFragmentDecryptor
{
    private readonly CencSampleDecryptor mSampleDecryptor;

    private readonly PiffSampleAuxiliaryOffsetBox? mSaio;

    private readonly PiffSampleAuxiliaryInformationBox? mSaiz;


    #region Properties
    
    /// <summary>
    /// Track ID for which this decryptor keeps the information.
    /// </summary>
    public uint TrackId { get; }
    

    /// <summary>
    /// The number of samples in the track fragment.
    /// </summary>
    public long SampleCount { get; }

    #endregion


    public CencFragmentDecryptor(
        PiffTrackFragmentBox traf,
        PiffTrackFragmentRunBox[] truns, PiffTrackBox[] tracks,
        PiffMovieBox moov,
        List<CencTrackDecryptor> decryptors, CencDecryptionContext ctx)
    {
        var tfhd = traf.FirstOfType<PiffTrackFragmentHeaderBox>();
        if (tfhd is null)
        {
            ctx.AddError($"No '{PiffReader.GetBoxName<PiffTrackFragmentHeaderBox>()}' for track.");
            return;
        }

        TrackId = tfhd.TrackId;
        var trak = tracks.FirstOrDefault(t => t.FirstOfType<PiffTrackHeaderBox>().TrackId == TrackId);
        if (trak is null)
        {
            ctx.AddError($"'{PiffReader.GetBoxName<PiffTrackHeaderBox>()}' for {TrackId} not found.");
            return;
        }

        var trex = moov.FirstOfType<PiffMovieExtendedBox>()?
                       .ChildrenOfType<PiffTrackExtendedBox>()
                       .FirstOrDefault(t => t.TrackId == TrackId);
        if (trex is null)
        {
            ctx.AddError($"'{PiffReader.GetBoxName<PiffTrackExtendedBox>()}' for {TrackId} not found.");
            return;
        }

        var decr = decryptors.FirstOrDefault(d => d.TrackId == TrackId);
        if (decr is null)
        {
            ctx.AddError($"{nameof(CencTrackDecryptor)} for {TrackId} not found.");
            return;
        }

        var index = (tfhd.Flags & PiffTrackFragmentHeaderBox.FlagsDescriptionIndexPresent) != 0
            ? tfhd.SampleDescriptionIndex
            : trex.DefaultDescriptionIndex;
        var sampleDescription = decr.GetSampleDescription(index);
        if (sampleDescription is null)
        {
            ctx.AddError($"Sample description # {index} for {TrackId} not found.");
            return;
        }

        mSampleDecryptor = new CencSampleDecryptor(sampleDescription, traf, decr.TrackKey, ctx);
        if (ctx.Messages.Any()) return;

        mSaio = traf.ChildrenOfType<PiffSampleAuxiliaryOffsetBox>()
                    .FirstOrDefault(s => s.AuxInfoType is null || s.AuxInfoType == "cenc");
        if (mSaio is null)
        {
            ctx.AddError($"'{PiffReader.GetBoxName<PiffSampleAuxiliaryOffsetBox>()}' for {TrackId} not found.");
            return;
        }

        mSaiz = traf.ChildrenOfType<PiffSampleAuxiliaryInformationBox>()
                    .FirstOrDefault(s => s.AuxInfoType is null || s.AuxInfoType == "cenc");
        if (mSaiz is null)
        {
            ctx.AddError($"'{PiffReader.GetBoxName<PiffSampleAuxiliaryInformationBox>()}' for {TrackId} not found.");
            return;
        }

        SampleCount = truns.Sum(t => t.SampleCount);

        var ivArray = new byte[mSampleDecryptor.IvSize * SampleCount];

        for (var trunIdx = 0; trunIdx < truns.Length; trunIdx++)
        {
            if (mSampleDecryptor.PerSampleIvSize > 0)
                Array.Copy(mSampleDecryptor.Senc.Items[trunIdx].InitVector, 0, ivArray, trunIdx * mSampleDecryptor.IvSize, mSampleDecryptor.IvSize);
            else
                Array.Copy(mSampleDecryptor.Tenc.ConstantInitVector, 0, ivArray, trunIdx * mSampleDecryptor.IvSize, mSampleDecryptor.IvSize);
        }
    }
}
