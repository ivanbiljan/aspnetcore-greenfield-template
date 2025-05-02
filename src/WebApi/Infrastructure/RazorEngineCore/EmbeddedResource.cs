using System.Reflection;

namespace WebApi.Infrastructure.RazorEngineCore;

internal static class EmbeddedResource
{
    public static string Read(Assembly assembly, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path must not be empty", nameof(relativePath));
        }

        var fullPath =
            $"{assembly.GetName().Name}.{relativePath.TrimStart('.').Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.')}";

        using var manifestResource = assembly.GetManifestResourceStream(fullPath) ??
                                     throw new ArgumentException(
                                         "Requested resource does not exist",
                                         nameof(relativePath)
                                     );

        using var streamReader = new StreamReader(manifestResource);

        return streamReader.ReadToEnd();
    }
}