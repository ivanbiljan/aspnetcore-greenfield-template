using System.Reflection;

namespace SourceGenerators;

public static class EmbeddedResource
{
    public static string Read(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path must not be empty", nameof(relativePath));
        }
        
        var fullPath =
            $"{Assembly.GetExecutingAssembly().GetName().Name}.{relativePath.TrimStart('.').Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.')}";
        
        using var manifestResource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullPath) ??
                                     throw new ArgumentException(
                                         "Requested resource does not exist",
                                         nameof(relativePath)
                                     );
        
        using var streamReader = new StreamReader(manifestResource);
        
        return streamReader.ReadToEnd();
    }
}