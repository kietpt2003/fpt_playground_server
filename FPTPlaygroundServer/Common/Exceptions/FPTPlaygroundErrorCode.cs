using System.Net;

namespace FPTPlaygroundServer.Common.Exceptions;

/**
    * Guidelines:
    *
    *   FPB: Business errors.
    *   FPV: Validation errors.
    *   FPS: Server errors.
    *   FPA: Authentication/Authorization errors.
    *
    */

public class FPTPlaygroundErrorCode
{
    public string Code { get; } = default!;
    public string Title { get; } = default!;
    public HttpStatusCode Status { get; }
    
    private FPTPlaygroundErrorCode(string code, string title, HttpStatusCode status)
    {
        Code = code;
        Title = title;
        Status = status;
    }

    public static readonly FPTPlaygroundErrorCode FPB_00 = new("FPB_00", "Error does not exist", HttpStatusCode.BadRequest);
    public static readonly FPTPlaygroundErrorCode FPB_01 = new("FPB_01", "Error exist", HttpStatusCode.BadRequest);
    public static readonly FPTPlaygroundErrorCode FPB_02 = new("FPB_02", "Invalid error", HttpStatusCode.BadRequest);
    public static readonly FPTPlaygroundErrorCode FPB_03 = new("FPB_03", "Authentication error", HttpStatusCode.BadRequest);

    public static readonly FPTPlaygroundErrorCode FPV_00 = new("FPV_00", "Syntax error", HttpStatusCode.BadRequest);

    public static readonly FPTPlaygroundErrorCode FPS_00 = new("FPS_00", "Server error", HttpStatusCode.InternalServerError);

    public static readonly FPTPlaygroundErrorCode FPA_00 = new("FPA_00", "Authentication error", HttpStatusCode.Unauthorized);
    public static readonly FPTPlaygroundErrorCode FPA_01 = new("FPA_01", "Permission error", HttpStatusCode.Unauthorized);
    public static readonly FPTPlaygroundErrorCode FPA_02 = new("FPA_02", "Too many requests", HttpStatusCode.TooManyRequests);
}