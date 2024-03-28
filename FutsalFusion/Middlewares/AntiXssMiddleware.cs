using Vereyon.Web;

namespace FutsalFusion.Middlewares;

public class AntiXssMiddleware
{
    private readonly RequestDelegate _next;

    public AntiXssMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.HasFormContentType)
        {
            var sanitizedValues = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

            foreach (var field in context.Request.Form)
            {
                var sanitizedValue = SanitizeHtml(field.Value);
                
                sanitizedValues[field.Key] = sanitizedValue;
            }

            var sanitizedForm = new FormCollection(sanitizedValues, context.Request.Form.Files);
            
            context.Request.Form = sanitizedForm;
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
    
    private string SanitizeHtml(string html)
    {
        var sanitizer = HtmlSanitizer.SimpleHtml5Sanitizer();
        
        return sanitizer.Sanitize(html);
    }
}
