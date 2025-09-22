using SharpDX;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Rendering.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 400)]
internal struct CameraBufferLayout
{
    public Matrix ModelMatrix;
    public Matrix ViewMatrix;
    public Matrix ProjectionMatrix;
    public Matrix ViewProjectionMatrix;
    public Matrix InvViewMatrix;
    public Matrix InvProjectionMatrix;
    public float  NearPlane;
    public float  FarPlane;
    private float PADDING1;
    private float PADDING2;

    public CameraBufferLayout(Matrix modelMatrix, Matrix viewMatrix, Matrix projectionMatrix, float nearPlane, float farPlane)
    {
        ModelMatrix         = modelMatrix;

        ViewMatrix          = viewMatrix;
        ProjectionMatrix    = projectionMatrix;

        ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
        ViewProjectionMatrix.Transpose();

        InvViewMatrix       = Matrix.Invert(viewMatrix);
        InvProjectionMatrix = Matrix.Invert(projectionMatrix);
        NearPlane           = nearPlane;
        FarPlane            = farPlane;

        PADDING1 = 0;
        PADDING2 = 0;
    }
}
