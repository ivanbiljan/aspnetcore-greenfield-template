namespace Greenfield.Tests.Infrastructure;

[CollectionDefinition(FixtureName)]
public sealed class IntegrationTestCollectionContainer : ICollectionFixture<CustomApplicationFactory>
{
    public const string FixtureName = "IntegrationTestFixture";
}