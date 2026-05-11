using System;
using System.IO;
using System.Reflection;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public class ResourceLoader : IResourceLoader
{
    public byte[] GetEmbeddedResourceBytes(string resourceName)
    {
        Assembly assembly = typeof(TheMirrorsEdgePlugin).Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        
        if (stream == null)
        {
            throw new ArgumentException($"Resource {resourceName} not found", nameof(resourceName));
        }

        byte[] returnBytes = new byte[stream.Length];

        stream.ReadExactly(returnBytes, 0, returnBytes.Length);

        return returnBytes;
    }
}