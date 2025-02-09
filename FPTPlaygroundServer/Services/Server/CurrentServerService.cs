namespace FPTPlaygroundServer.Services.Server;

public class CurrentServerService(IHttpContextAccessor httpContextAccessor)
{
    public string ServerUrl
        => string.Concat(httpContextAccessor?.HttpContext?.Request.Scheme, "://",
            httpContextAccessor?.HttpContext?.Request.Host.ToUriComponent())
        ?? throw new Exception("Server url not exist.");
}
