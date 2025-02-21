﻿using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Paginations;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.FaceValues.Mappers;
using FPTPlaygroundServer.Features.Servers.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;

namespace FPTPlaygroundServer.Features.FaceValues;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class GetFaceValues : ControllerBase
{
    public new class Request : PageRequest
    {
        public SortDir SortOrder { get; set; }
        public string? SortColumn { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("face-values")]
    [Tags("FaceValue")]
    [SwaggerOperation(Summary = "Get Face Values",
        Description = """
        This API is for retrieving face values

        `SortColumn` (optional): coinValue, diamondValue, vndValue, createdAt
        """
    )]
    [ProducesResponseType(typeof(PageList<ServerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context)
    {
        var query = context.FaceValues.AsQueryable();

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
            .Select(c => c.ToFaceValueResponse())
            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<FaceValue, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "coinvalue" => c => c.CoinValue,
            "diamondvalue" => c => c.DiamondValue,
            "vndvalue" => c => c.VNDValue,
            "createdat" => c => c.CreatedAt,
            _ => c => c.Id
        };
    }
}
