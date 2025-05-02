namespace WebApi.Infrastructure.Web;

internal sealed class RequestIdDecoratorMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    
    public Task Invoke(HttpContext context)
    {
        context.Response.Headers["RequestId"] = context.TraceIdentifier;
        
        return _next(context);
    }
}