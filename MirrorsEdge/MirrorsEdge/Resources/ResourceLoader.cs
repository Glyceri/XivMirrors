using MirrorsEdge.mirrorsedge.Memory;
using MirrorsEdge.mirrorsedge.Resources.Interfaces;
using System;
using System.IO;
using System.Reflection;

namespace MirrorsEdge.MirrorsEdge.Resources;

internal class ResourceLoader
{
    private readonly DirectXData DirectXData;

    public ResourceLoader(DirectXData directXData)
    {
        DirectXData = directXData;
    }

    public byte[] GetEmbeddedResourceBytes(string resourceName)
    {
        Assembly assembly = typeof(MirrorsEdgePlugin).Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        
        if (stream == null)
        {
            throw new ArgumentException($"Resource {resourceName} not found", nameof(resourceName));
        }

        byte[] returnBytes = new byte[stream.Length];

        stream.ReadExactly(returnBytes, 0, returnBytes.Length);

        return returnBytes;
    }

    public IRenderTarget CreateRenderTarget()
    {
        throw new NotImplementedException();
    }
}
