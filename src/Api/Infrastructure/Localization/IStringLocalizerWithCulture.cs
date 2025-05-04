using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure.Localization;

internal interface IStringLocalizerWithCulture : IStringLocalizer
{
    new LocalizedString this[string resourceName] { get; }

    LocalizedString this[string resourceName, CultureInfo localizationCulture] { get; }
}

internal interface IStringLocalizerWithCulture<out T> : IStringLocalizerWithCulture
{
}

internal interface IStringLocalizerWithCultureFactory : IStringLocalizerFactory
{
    new IStringLocalizerWithCulture Create(Type resourceSource);

    IStringLocalizerWithCulture Create(Type resourceSource, CultureInfo defaultLocalizationCulture);
}

internal sealed class ResourceManagerStringLocalizerWithCulture(
    ResourceManager resourceManager,
    Assembly resourceAssembly,
    string baseName,
    IResourceNamesCache resourceNamesCache,
    ILogger logger,
    CultureInfo? defaultLocalizationCulture
) : ResourceManagerStringLocalizer(resourceManager, resourceAssembly, baseName, resourceNamesCache, logger),
    IStringLocalizerWithCulture
{
    private readonly string _baseName = baseName;

    public LocalizedString this[string resourceName, CultureInfo localizationCulture]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty(resourceName);

            var resourceValue = GetStringSafely(resourceName, localizationCulture);

            return new LocalizedString(resourceName, resourceValue ?? resourceName, resourceValue is null, _baseName);
        }
    }

    public new LocalizedString this[string resourceName]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty(resourceName);

            var resourceValue = GetStringSafely(
                resourceName,
                defaultLocalizationCulture ?? Thread.CurrentThread.CurrentUICulture
            );

            return new LocalizedString(resourceName, resourceValue ?? resourceName, resourceValue is null, _baseName);
        }
    }
}

[SuppressMessage("Globalization", "CA1304:Specify CultureInfo")]
internal sealed class StringLocalizerWithCulture<TResourceSource>(IStringLocalizerWithCultureFactory factory)
    : StringLocalizer<TResourceSource>(factory), IStringLocalizerWithCulture<TResourceSource>
{
    private readonly IStringLocalizerWithCulture _localizer = factory.Create(typeof(TResourceSource));

    public LocalizedString this[string resourceName, CultureInfo localizationCulture] =>
        _localizer[resourceName, localizationCulture];
}

internal sealed class ResourceManagerStringLocalizerWithCultureFactory(
    IOptions<LocalizationOptions> localizationOptions,
    ILoggerFactory loggerFactory
) : ResourceManagerStringLocalizerFactory(localizationOptions, loggerFactory), IStringLocalizerWithCultureFactory
{
    private readonly ConcurrentDictionary<string, ResourceManagerStringLocalizerWithCulture>
        _localizerCache = new();

    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly IResourceNamesCache _resourceNamesCache = new ResourceNamesCache();

    public new IStringLocalizerWithCulture Create(Type resourceSource)
    {
        return Create(resourceSource, null);
    }

    public IStringLocalizerWithCulture Create(Type resourceSource, CultureInfo? defaultLocalizationCulture)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        var typeInfo = resourceSource.GetTypeInfo();
        var baseName = GetResourcePrefix(typeInfo);
        var assembly = typeInfo.Assembly;

        var localizationKey = defaultLocalizationCulture is null
            ? baseName
            : $"{baseName}-{defaultLocalizationCulture.Name}";

        return _localizerCache.GetOrAdd(
            localizationKey,
            _ => CreateLocalizer(assembly, baseName, defaultLocalizationCulture)
        );
    }

    private ResourceManagerStringLocalizerWithCulture CreateLocalizer(
        Assembly assembly,
        string baseName,
        CultureInfo? defaultLocalizationCulture
    )
    {
        return new ResourceManagerStringLocalizerWithCulture(
            new ResourceManager(baseName, assembly),
            assembly,
            baseName,
            _resourceNamesCache,
            _loggerFactory.CreateLogger<ResourceManagerStringLocalizer>(),
            defaultLocalizationCulture
        );
    }
}