using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Common.Paginations;

public class PageRequest
{
    public const int MaxPageSize = 100;
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

public class PagedRequestValidator<T> : AbstractValidator<T> where T : PageRequest
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(PageRequest.MaxPageSize);
    }
}

public record PageList<T>(List<T> Items, int Page, int PageSize, int TotalItems)
{
    public bool HasNextPage => Page * PageSize < TotalItems;
    public bool HasPreviousPage => Page > 1;
}

public static class PaginationDatabaseExtensions
{
    public static async Task<PageList<TResponse>> ToPagedListAsync<TRequest, TResponse>(this IQueryable<TResponse> query, TRequest request) where TRequest : PageRequest
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 10;

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, PageRequest.MaxPageSize);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageList<TResponse>(items, page, pageSize, totalItems);
    }
}
