namespace ICDS.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    
    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context)
    {
        var originalPath = context.Request.Path;
        
        var decodedPath = Uri.UnescapeDataString(originalPath);

        context.Request.Path = decodedPath;

        await _next(context);
        
        var requestCode = context.Response.StatusCode;
        
        await ErrorHandling(context, requestCode);
    }
    
    private static Task ErrorHandling(HttpContext context, int HttpErrorCode)
    {
        var errorMessage = "";

        if (HttpErrorCode is not (400 or 403 or 404 or 405 or 408 or 408 or 500 or 502))
            return context.Response.WriteAsync(errorMessage);
        
        switch (HttpErrorCode)
        {
            case 400:
                context.Response.ContentType = "text/plain";
                context.Response.Redirect("/Handler/BadRequestView");
                break;
            case 403:
                context.Response.ContentType = "text/plain";
                context.Response.Redirect("/Handler/Forbidden");
                break;
            case 404:
                context.Response.ContentType = "text/plain";
                context.Response.Redirect("/Handler/NotFoundView");
                break;
            case 405:
                context.Response.ContentType = "text/plain";
                context.Items["ErrorMessage"] = "A request method is not supported for the requested resource";
                break;
            case 408:
                context.Response.ContentType = "text/plain";
                context.Items["ErrorMessage"] = "The client did not produce a request within the time that the server was prepared to wait.";
                break;
            case 500:
                context.Response.ContentType = "text/plain";
                context.Items["ErrorMessage"] = "An internal error has occured in the server side,";
                break;
            case 502:
                context.Response.ContentType = "text/plain";
                context.Items["ErrorMessage"] = "The server was acting as a gateway or proxy and received an invalid response from the upstream server.";
                break;
        }
        
        return context.Response.WriteAsync(errorMessage);
    }    
}