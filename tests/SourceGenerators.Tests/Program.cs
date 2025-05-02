// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");

new ServiceCollection().AutoConfigureOptions();

namespace SourceGenerators.IntegrationTests
{
    [ConfigureOptions("Email")]
    internal sealed class EmailOptions
    {
    }
}