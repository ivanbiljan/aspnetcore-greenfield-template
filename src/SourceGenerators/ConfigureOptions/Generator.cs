﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace SourceGenerators.ConfigureOptions;

[Generator]
public class Generator : IIncrementalGenerator
{
    private const string Namespace = "SourceGenerators";
    private const string AttributeName = "ConfigureOptionsAttribute";

    private const string ConfigureOptionsAttributeSource = $$"""
                                                             // <auto-generated/>

                                                             namespace {{Namespace}};

                                                             [System.AttributeUsage(System.AttributeTargets.Class)]
                                                             public sealed class {{AttributeName}}(string sectionName) : System.Attribute
                                                             {
                                                                public string SectionName { get; } = sectionName;
                                                             }
                                                             """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postInitializationContext =>
            {
                postInitializationContext.AddSource(
                    "ConfigureOptionsAttribute.g.cs",
                    SourceText.From(ConfigureOptionsAttributeSource, Encoding.UTF8)
                );
            }
        );

        var classDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{Namespace}.{AttributeName}",
                static (_, _) => true,
                static (ctx, _) =>
                {
                    var className = ctx.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var sectionName = (string) ctx.Attributes[0]
                        .ConstructorArguments
                        .FirstOrDefault()
                        .Value!;

                    return new KeyValuePair<string, string>(className, sectionName);
                }
            )
            .Collect();

        var template = EmbeddedResource.Read("ConfigureOptions/Template.sbntxt");
        context.RegisterSourceOutput(
            classDeclarations,
            (ctx, options) =>
            {
                var output = Template.Parse(template).Render(new {options});
                ctx.AddSource("ConfigureOptions.g.cs", SourceText.From(output, Encoding.UTF8));
            }
        );
    }
}