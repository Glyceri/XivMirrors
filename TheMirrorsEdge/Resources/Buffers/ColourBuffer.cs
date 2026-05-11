using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Resources.Buffers;

public class ColourBuffer(DirectXData directXData) 
    : BasicBuffer<ColourBufferLayout>(directXData);