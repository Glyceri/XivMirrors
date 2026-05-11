namespace TheMirrorsEdge.Services.Interfaces;

public interface IResourceLoader
{
    byte[] GetEmbeddedResourceBytes(string resourceName);
}