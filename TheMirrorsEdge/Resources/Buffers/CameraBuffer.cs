using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Resources.Buffers;

public class CameraBuffer(DirectXData directXData) 
    : BasicBuffer<CameraBufferLayout>(directXData);