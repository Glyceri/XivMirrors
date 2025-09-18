using MirrorsEdge.XIVMirrors.Hooking.Structs;
using Penumbra.String;

namespace MirrorsEdge.XIVMirrors.Services.Wrappers;

internal unsafe class Utils
{
    // YOINKERS from: https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/InteropUtils.cs#L12
    // Made it verbose in the way I like it
    public int ComputeHash(CiByteString path, GetResourceParameters* resParams)
    {
        if (resParams == null)
        {
            return path.Crc32;
        }

        if (!resParams->IsPartialRead)
        {
            return path.Crc32;
        }

        CiByteString? SegmentOffsetString;
        CiByteString? SegmentLengthString;

        if (CiByteString.FromString(resParams->SegmentOffset.ToString("x"), out CiByteString? outcomeSegmentOffsetString, MetaDataComputation.None))
        {
            SegmentOffsetString = outcomeSegmentOffsetString;
        }
        else
        {
            SegmentOffsetString = CiByteString.Empty;
        }

        if (CiByteString.FromString(resParams->SegmentLength.ToString("x"), out CiByteString? outcomeSegmentLengthString, MetaDataComputation.None))
        {
            SegmentLengthString = outcomeSegmentLengthString;
        }
        else
        {
            SegmentLengthString = CiByteString.Empty;
        }

        CiByteString outcomeString = CiByteString.Join((byte)'.', path, SegmentOffsetString, SegmentLengthString);

        int crc32Outcome = outcomeString.Crc32;

        return crc32Outcome;
    }
}
