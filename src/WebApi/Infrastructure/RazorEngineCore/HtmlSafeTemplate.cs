using System.Web;
using RazorEngineCore;

namespace WebApi.Infrastructure.RazorEngineCore;

internal class HtmlSafeTemplate<T> : RazorEngineTemplateBase<T>
{
    public static object Raw(object? value)
    {
        return new RawContent(value);
    }

    public override void Write(object? obj = null)
    {
        var value = obj is RawContent rawContent
            ? rawContent.Value
            : HttpUtility.HtmlEncode(obj);

        base.Write(value);
    }

    public override void WriteAttributeValue(
        string prefix,
        int prefixOffset,
        object? value,
        int valueOffset,
        int valueLength,
        bool isLiteral
    )
    {
        value = value is RawContent rawContent
            ? rawContent.Value
            : HttpUtility.HtmlAttributeEncode(value?.ToString());

        base.WriteAttributeValue(prefix, prefixOffset, value, valueOffset, valueLength, isLiteral);
    }

    private sealed class RawContent(object? value)
    {
        public object? Value { get; } = value;
    }
}