namespace Greenfield.Tests.Extensions;

[CollectionDefinition(FixtureName)]
public sealed class IntegrationTestCollectionContainer : ICollectionFixture<CustomApplicationFactory>
{
    public const string FixtureName = "IntegrationTestFixture";
}