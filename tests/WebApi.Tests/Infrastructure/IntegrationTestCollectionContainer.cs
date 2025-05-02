using System.Diagnostics.CodeAnalysis;

namespace WebApi.Tests.Infrastructure;

[CollectionDefinition(FixtureName)]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "xUnit collection definition classes must be public")]
public sealed class IntegrationTestCollectionContainer : ICollectionFixture<CustomApplicationFactory>
{
    public const string FixtureName = "IntegrationTestFixture";
}