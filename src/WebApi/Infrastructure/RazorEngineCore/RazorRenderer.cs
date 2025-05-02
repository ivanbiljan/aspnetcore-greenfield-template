using System.Reflection;
using RazorEngineCore;

namespace WebApi.Infrastructure.RazorEngineCore;

internal interface IRazorRenderer
{
    Task<string> RenderAsync<TModel>(string templatePath, TModel model);

    Task<string> RenderAsync<TModel>(Assembly assembly, string templatePath, TModel model);
}

[RegisterSingleton]
internal sealed class RazorRenderer : IRazorRenderer
{
    private readonly RazorEngine _razorEngine = new();

    public Task<string> RenderAsync<TModel>(string templatePath, TModel model)
    {
        return RenderAsync(Assembly.GetCallingAssembly(), templatePath, model);
    }
    
    public async Task<string> RenderAsync<TModel>(Assembly assembly, string templatePath, TModel model)
    {
        var templateText = EmbeddedResource.Read(assembly, templatePath);
        var template = await _razorEngine.CompileAsync<HtmlSafeTemplate<TModel>>(templateText);

        return await template.RunAsync(
            instance => { instance.Model = model; }
        );
    }
}